using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Entities;
using Hangfire;
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
using ShopImport.Service.Infrastructure.Module.Models;
using System.Collections.Concurrent;

namespace ShopImport.Service.Hangfire;

internal class ShopImportServiceManager(IEnumerable<IImportServiceFactory> shopServiceFactories,
    IShopDataService shopDataService,
    IImportServiceLogFactory importServiceLogFactory,
    ILoggerFactory loggerFactory,
    JobExecuteOptions jobExecuteOptions) 
    : IShopImportServiceJobManager
{
    private readonly IEnumerable<IImportServiceFactory> _shopServiceFactories = shopServiceFactories;

    private readonly IShopDataService _shopDataService = shopDataService;

    private readonly IImportServiceLogFactory _importServiceLogFactory = importServiceLogFactory;

    private readonly ILoggerFactory _loggerFactory = loggerFactory;

    private readonly JobExecuteOptions _jobExecuteOptions = jobExecuteOptions;

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
        var serviceJob = GetServiceJob(guid);

        var startTask = StartService(serviceJob, cancellationToken);

        return (serviceJob.ImportService, startTask);
    }

    private async Task StartService(IImportServiceJob serviceJob, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(serviceJob.JobId))
            throw new InvalidOperationException($"Job for service {serviceJob.ImportService.Name} already run.");

        var executionJobs = await serviceJob.GetExecutionServiceJobs();
        executionJobs.ToList().ForEach(j => _allServiceJobs.TryAdd(j.Key, j.Value));        

        foreach (var executionJob in executionJobs)
        {
            var jobId = BackgroundJob.Enqueue<IImportServiceJobManager>(serviceJobManager => serviceJobManager.Execute(executionJob.Value.Guid, cancellationToken));
            executionJob.Value.JobId = jobId;
        }        
    }

    public async Task StartAndDisposeAsync(Guid guid, CancellationToken cancellationToken)
    {
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        try
        {
            var serviceJob = GetServiceJob(guid);        
            await serviceJob.ImportService.Start(linkedCts.Token);
        }
        finally
        {
            linkedCts.Dispose();
        }
    }
    
    private IImportServiceJob GetServiceJob(Guid guid)
    {
        return !_coreServiceJobs.TryGetValue(guid, out var serviceJob)
            ? throw new InvalidOperationException($"Service with id {guid} not found.")
            : serviceJob;
    }

    public (IImportService service, Task startTask) StopServiceTask(Guid guid, CancellationToken cancellationToken)
    {
        if (!_coreServiceJobs.TryGetValue(guid, out var serviceJob))
            throw new InvalidOperationException($"Service with id {guid} not found.");
        
        return (serviceJob.ImportService, StopServiceAsync(serviceJob, cancellationToken));
    }

    public IEnumerable<(Guid guid, IImportService service, Task stopTask)> StopAllServicesTask(CancellationToken cancellationToken)
    {
        return _allServiceJobs.Select(si => (si.Key, si.Value.ImportService, StopServiceAsync(si.Value, cancellationToken)));
    }

    private static async Task StopServiceAsync(IImportServiceJob serviceJob, CancellationToken cancellationToken)
    {
        await serviceJob.ImportService.Stop(cancellationToken);
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