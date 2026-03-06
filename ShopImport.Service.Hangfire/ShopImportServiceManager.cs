using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Entities;
using Hangfire;
using Hangfire.States;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Service;
using Import.Service.Commands.Models;
using Import.Service.Infrastructure;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;
using Shop.Interfaces;
using ShopImport.Service.Hangfire.Infrastructure;
using ShopImport.Service.Hangfire.Models;
using System.Collections.Concurrent;

namespace ShopImport.Service.Hangfire;

internal class ShopImportServiceManager(IEnumerable<IImportServiceFactory> shopServiceFactories,
    IShopDataService shopDataService,
    IImportServiceLogFactory importServiceLogFactory,
    ILoggerFactory loggerFactory,
    JobExecuteOptions jobExecuteOptions,
    IBackgroundJobClient backgroundJobClient) 
    : IShopImportServiceJobManager
{
    private readonly IEnumerable<IImportServiceFactory> _shopServiceFactories = shopServiceFactories;

    private readonly IShopDataService _shopDataService = shopDataService;

    private readonly IImportServiceLogFactory _importServiceLogFactory = importServiceLogFactory;

    private readonly ILoggerFactory _loggerFactory = loggerFactory;

    private readonly JobExecuteOptions _jobExecuteOptions = jobExecuteOptions;

    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    private readonly ConcurrentDictionary<Guid, IImportServiceJob> _allServiceJobs = [];

    private readonly ConcurrentDictionary<Guid, IImportServiceJob> _coreServiceJobs = [];

    private readonly SemaphoreSlim _addServiceSemaphoreSlim = new(1);

    private List<IImportSource> ShopModels { get; } = [];

    public async Task<(Guid guid, IImportService service)> AddShopImportService(string name, IShopImportSettings shopImportSettings, CancellationToken cancellationToken = default)
    {
        await _addServiceSemaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            var shopModel = await _shopDataService.GetShopModelAsync(shopImportSettings, cancellationToken);
            if (!ShopModels.Any(s => s.Name == ((IImportSource)shopModel).Name))
                ShopModels.Add(shopModel);

            var serviceJob = CreateImportServiceJob(name, shopImportSettings, shopModel);
            _coreServiceJobs.TryAdd(serviceJob.Guid, serviceJob);

            var executionJobs = await serviceJob.GetExecutionServiceJobs();
            executionJobs.ToList().ForEach(j => _allServiceJobs.TryAdd(j.Key, j.Value));

            foreach (var executionJob in executionJobs)
            {
                var jobId = BackgroundJob.Enqueue<IImportServiceJobManager>(_jobExecuteOptions.WaitingQueue,
                     serviceJobManager => serviceJobManager.Execute(executionJob.Value.Guid, cancellationToken));
                executionJob.Value.JobId = jobId;
            }

            return (serviceJob.Guid, serviceJob.ImportService);
        }
        finally
        {
            _addServiceSemaphoreSlim.Release();
        }
    }

    private IImportServiceJob CreateImportServiceJob(string name, IShopImportSettings shopImportSettings, IShopModel source)
    {
        var importServiceSettings = shopImportSettings.GetImportService()
            ?? throw new InvalidOperationException($"Import settings {name} does not contain the import service settings");

        var serviceFactory = _shopServiceFactories.FirstOrDefault(f => f.ServiceImplementationType.Name == importServiceSettings.ImplementationTypeName)
            ?? throw new InvalidOperationException($"Service type {importServiceSettings.ImplementationTypeName} not found.");

        if(!shopImportSettings.IsAggregate)
            return new ShopImportServiceJob(name, source, shopImportSettings, serviceFactory);

        var logger = _loggerFactory.CreateLogger<AggregateImportService>();
        var serviceLogger = _importServiceLogFactory.GetLogger(logger, name, source, shopImportSettings);

        return new AggregateShopImportServiceJob(serviceLogger, name, source, shopImportSettings, serviceFactory);
    }

    Task<(Guid guid, IImportService service)> IServiceManager.AddImportService(string name, IImportSettings importSettings,
        CancellationToken cancellationToken)
    {
        if (importSettings is not IShopImportSettings shopImportSettings)
            throw new InvalidOperationException($"Invalid import settings type {importSettings.GetType().Name}");

        return AddShopImportService(name, shopImportSettings, cancellationToken);
    }

    public (IImportService service, Task startTask) StartServiceTask(Guid guid, CancellationToken cancellationToken)
    {
        var serviceJob = GetCoreServiceJob(guid);

        var startTask = StartService(guid, cancellationToken);

        return (serviceJob.ImportService, startTask);
    }

    private Task StartService(Guid serviceJobId, CancellationToken cancellationToken)
    {
        //todo
        var serviceJob = GetCoreServiceJob(serviceJobId);
        _backgroundJobClient.ChangeState(serviceJob.JobId, new EnqueuedState(_jobExecuteOptions.ProcessingQueue));

        var childJobs = _allServiceJobs.Where(j => j.Value.ParentId == serviceJobId).ToList();
        childJobs.ForEach(job =>
        _backgroundJobClient.ChangeState(job.Value.JobId, new EnqueuedState(_jobExecuteOptions.ProcessingQueue)));

        return Task.CompletedTask;
    }

    public async Task StartAndDisposeAsync(Guid guid, CancellationToken cancellationToken)
    {
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        try
        {
            var serviceJob = GetExecutingServiceJob(guid);        
            await serviceJob.ImportService.Start(linkedCts.Token);
        }
        finally
        {
            linkedCts.Dispose();
        }
    }
    
    private IImportServiceJob GetCoreServiceJob(Guid guid)
    {
        return !_coreServiceJobs.TryGetValue(guid, out var serviceJob)
            ? throw new InvalidOperationException($"Service with id {guid} not found.")
            : serviceJob;
    }

    private IImportServiceJob GetExecutingServiceJob(Guid guid)
    {
        return !_allServiceJobs.TryGetValue(guid, out var serviceJob)
            ? throw new InvalidOperationException($"Service with id {guid} not found.")
            : serviceJob;
    }

    private List<IImportServiceJob> GetExecutingServiceJobs(Guid coreJobGuid)
    {
        return [.. _allServiceJobs.Where(j=>j.Key == coreJobGuid || j.Value.ParentId == coreJobGuid).Select(j=>j.Value)];
    }

    public (IImportService service, Task startTask) StopServiceTask(Guid guid, CancellationToken cancellationToken)
    {
        var serviceJob = GetCoreServiceJob(guid);

        return (serviceJob.ImportService, StopServiceAsync(serviceJob, cancellationToken));
    }

    public IEnumerable<(Guid guid, IImportService service, Task stopTask)> StopAllServicesTask(CancellationToken cancellationToken)
    {
        return _allServiceJobs.Select(si => (si.Key, si.Value.ImportService, StopServiceAsync(si.Value, cancellationToken)));
    }

    private Task StopServiceAsync(IImportServiceJob serviceJob, CancellationToken cancellationToken)
    {
        var executingTasks = GetExecutingServiceJobs(serviceJob.Guid).Select(e=>e.ImportService.Stop(cancellationToken));
        return Task.WhenAll(executingTasks);
    }

    private readonly Lock _shopModelsLock = new();

    public async Task AddShopCategory(ShopCategory shopCategory)
    {
        ProductShopModel? shop;
        IProductShopCategory? productShopCategory;
        lock (_shopModelsLock)
        {
            shop = ShopModels.OfType<ProductShopModel>().FirstOrDefault(s => s.Id == shopCategory.ShopId);

            if (shop?.RootCategories.Any(c => c.ItemId == shopCategory.ItemId) != true)
                return;

            productShopCategory = shopCategory.ToProductShopCategoryModel();
            shop?.Categories.Add(productShopCategory);
        }

        //todo
        if (_coreServiceJobs.FirstOrDefault(s => s.Value.SourceId == shop?.Id
                    && s.Value.ImportService is IListener<IProductShopCategory> shopCategoryListener).Value.ImportService
            is IListener<IProductShopCategory> shopCategoryListener)
            await shopCategoryListener.On(productShopCategory);
    }

    Task IImportServiceJobManager.Execute(Guid guid, CancellationToken cancellationToken)
    {
        return StartAndDisposeAsync(guid, cancellationToken);
    }
}