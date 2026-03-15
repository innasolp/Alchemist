using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Entities;
using Hangfire.Server;
using Hangfire.Tags;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Service.Commands.Models;
using Import.Service.Infrastructure;
using Import.Settings.Interfaces;
using Shop.Interfaces;
using ShopImport.Service.Hangfire.Infrastructure;
using ShopImport.Service.Hangfire.Infrastructure.JobExecutors;
using ShopImport.Service.Hangfire.Models;
using System.Collections.Concurrent;

namespace ShopImport.Service.Hangfire;

internal class HangfireShopImportServiceManager(IEnumerable<IImportServiceFactory> shopServiceFactories,
    IShopDataService shopDataService,
    IImportServiceJobFactory importServiceJobFactory,
    IJobExecuteManager jobExecuteManager,
    JobExecuteOptions jobExecuteOptions) 
    : IShopImportServiceJobManager
{
    private readonly IEnumerable<IImportServiceFactory> _shopServiceFactories = shopServiceFactories;

    private readonly IShopDataService _shopDataService = shopDataService;

    private readonly IImportServiceJobFactory _importServiceJobFactory = importServiceJobFactory;

    private readonly IJobExecuteManager _jobExecuteManager = jobExecuteManager;

    private readonly JobExecuteOptions _jobExecuteOptions = jobExecuteOptions;

    private readonly ConcurrentDictionary<Guid, IImportServiceJob> _allServiceJobs = [];

    private readonly ConcurrentDictionary<Guid, IImportServiceJob> _coreServiceJobs = [];

    private readonly ConcurrentDictionary<Guid, AsyncEventHandler<ConnectedAsyncEventArgs>> _serviceConnectedHandlers = [];

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
            _coreServiceJobs.TryAdd(serviceJob.Id, serviceJob);

            var executionJobs = await serviceJob.GetExecutionServiceJobs();
            executionJobs.ToList().ForEach(j => _allServiceJobs.TryAdd(j.Key, j.Value));

            SubscribeServiceToFailedHandler(serviceJob);

            await _jobExecuteManager.Enqueue(serviceJob, _jobExecuteOptions, cancellationToken);

            return (serviceJob.Id, serviceJob.ImportService);
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

        return _importServiceJobFactory.CreateServiceJob(serviceFactory, shopImportSettings, name, source, shopImportSettings.IsAggregate);
    }

    private void SubscribeServiceToFailedHandler(IImportServiceJob importServiceJob)
    {
        async Task serviceConnectedEventHandler (object sender, ConnectedAsyncEventArgs args) 
            => await ImportServiceConnectedAsync(sender, args, importServiceJob.Id);

        if (_serviceConnectedHandlers.TryAdd(importServiceJob.Id, serviceConnectedEventHandler))
            importServiceJob.ImportService.ConnectedAsync += serviceConnectedEventHandler;

        var childJobs = _allServiceJobs.Where(j => j.Value.ParentId == importServiceJob.Id).ToList();
        foreach(var childJob in childJobs)
        {
            async Task childConnectedEventHandler (object sender, ConnectedAsyncEventArgs args) 
                => await ImportServiceConnectedAsync(sender, args, childJob.Value.Id);

            if (_serviceConnectedHandlers.TryAdd(childJob.Value.Id, childConnectedEventHandler))
                childJob.Value.ImportService.ConnectedAsync += childConnectedEventHandler;
        }            
    }

    private async Task ImportServiceConnectedAsync(object sender, ConnectedAsyncEventArgs eventArgs, Guid serviceJobId)
    {
        if (sender is not IImportService importService)
            throw new InvalidOperationException($"Invalid sender type {sender.GetType().Name}. Sender must be assignable to {nameof(IImportService)}");

        if(!eventArgs.Success && eventArgs.Exception != null
            && _allServiceJobs.TryGetValue(serviceJobId, out var importServiceJob) && !string.IsNullOrEmpty(importServiceJob.JobId))
        {
            await _jobExecuteManager.StopWithFailedState(importServiceJob, eventArgs.Exception, _jobExecuteOptions, CancellationToken.None);
        }
    }

    Task<(Guid guid, IImportService service)> IServiceManager.AddImportService(string name, IImportSettings importSettings,
        CancellationToken cancellationToken)
    {
        if (importSettings is not IShopImportSettings shopImportSettings)
            throw new InvalidOperationException($"Invalid import settings type {importSettings.GetType().Name}");

        return AddShopImportService(name, shopImportSettings, cancellationToken);
    }

    public (IImportService service, Task startTask) StartServiceTask(Guid guid, CancellationToken cancellationToken = default)
    {
        var serviceJob = GetCoreServiceJob(guid);

        var startTask = StartService(serviceJob, cancellationToken);

        return (serviceJob.ImportService, Task.Run(() => startTask, cancellationToken));
    }

    private Task StartService(IImportServiceJob serviceJob, CancellationToken cancellationToken)
    {
        return _jobExecuteManager.Execute(serviceJob, _jobExecuteOptions, cancellationToken);
    }

    private static async Task StartServiceAsync(IImportService importService, CancellationToken cancellationToken)
    {
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        try
        {                
            await importService.Start(linkedCts.Token);
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
        var executingTasks = GetExecutingServiceJobs(serviceJob.Id).Select(e=>e.ImportService.Stop(cancellationToken));
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

    Task IHagfireServiceJobManager.Execute(Guid guid, string displayName, CancellationToken cancellationToken, PerformContext? performContext)
    {
        var linkedTokenSource =  CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
            performContext?.CancellationToken.ShutdownToken ?? default);
        var serviceJob = GetExecutingServiceJob(guid);

        if (performContext != null)
        {
            var parentServiceJob = serviceJob.ParentId.HasValue
                && _coreServiceJobs.TryGetValue(serviceJob.ParentId.Value, out var parentJob)
                && parentJob != null
                 ? parentJob
                 : serviceJob;
            performContext.AddTags(parentServiceJob.ImportService.Name);
        }

        try
        {
            var startTask = StartServiceAsync(serviceJob.ImportService, linkedTokenSource.Token);

            if(performContext != null) serviceJob.JobId = performContext.BackgroundJob.Id;

            return startTask;
        }
        finally
        {
            linkedTokenSource.Dispose();
        }
    }
}