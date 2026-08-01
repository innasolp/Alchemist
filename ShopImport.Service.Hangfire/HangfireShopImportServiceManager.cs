using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Entities;
using Hangfire;
using Hangfire.AggregateJobs;
using Hangfire.AggregateJobs.ChildJobStorages;
using Hangfire.Server;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Service.Infrastructure;
using Import.Settings.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Shop.Interfaces;
using ShopImport.Service.Hangfire.Infrastructure;
using ShopImport.Service.Hangfire.Models;
using System.Collections.Concurrent;

namespace ShopImport.Service.Hangfire;

internal class HangfireShopImportServiceManager(IEnumerable<IImportServiceFactory> shopServiceFactories,
    IShopDataService shopDataService,
    IImportServiceJobFactory importServiceJobFactory,
    IJobExecuteManager jobExecuteManager,
    AggregateServerSettings aggregateServerSettings,
    IEnumerable<IChildJobEnricher<IImportServiceJob>> childJobEnrichers,
    IServiceScopeFactory serviceScopeFactory)
    : IShopImportServiceJobManager, IAsyncDisposable
{
    private readonly IEnumerable<IImportServiceFactory> _shopServiceFactories = shopServiceFactories;

    private readonly IShopDataService _shopDataService = shopDataService;

    private readonly IImportServiceJobFactory _importServiceJobFactory = importServiceJobFactory;

    private readonly IJobExecuteManager _jobExecuteManager = jobExecuteManager;

    private readonly AggregateServerSettings _aggregateServerSettings = aggregateServerSettings;

    private readonly IEnumerable<IChildJobEnricher<IImportServiceJob>> _childJobEnrichers = childJobEnrichers;

    private readonly ConcurrentDictionary<Guid, IImportServiceJob> _allServiceJobs = [];

    private readonly ConcurrentDictionary<Guid, IImportServiceJob> _coreServiceJobs = [];

    private readonly ConcurrentDictionary<Guid, AsyncEventHandler<ConnectedAsyncEventArgs>> _serviceConnectedHandlers = [];

    private readonly SemaphoreSlim _serviceSemaphoreSlim = new(1);

    private List<IShopModel> ShopModels { get; } = [];

    private readonly List<ShopCategory> _waitingShopCategories = [];

    public async Task<(Guid guid, IImportService service)> AddShopImportService(string name, IShopImportSettings shopImportSettings, CancellationToken cancellationToken = default)
    {
        await _serviceSemaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            IShopModel shopModel = await GetShopModelAsync(shopImportSettings, cancellationToken);

            var serviceJob = CreateImportServiceJob(name, shopImportSettings, shopModel);
            _coreServiceJobs.TryAdd(serviceJob.Id, serviceJob);

            var executionJobs = await serviceJob.GetExecutionServiceJobs();
            executionJobs.ToList().ForEach(j => _allServiceJobs.TryAdd(j.Key, j.Value));

            using var scope = serviceScopeFactory.CreateScope();
            var childJobStorage = scope.ServiceProvider.GetRequiredService<IAggregateJobStorage>();
            var childJobs = await serviceJob.GetСhildJobs();
                
            var coreJobEntry = await childJobStorage.GetJobEntryByExecutionIdAsync(serviceJob.Id.ToString(), cancellationToken);
            if (coreJobEntry != null)
            {
                foreach (var childJob in childJobs)
                {
                    var childJobEntry = await childJobStorage.GetJobEntryByExecutionIdAsync(childJob.Id.ToString(), cancellationToken);
                    if (childJobEntry != null)
                        continue;

                    await EnqueueChildJobAsync(serviceJob, childJob, cancellationToken);
                }

            }
            else
            {
                await EnqueueCoreServiceJobAsync(serviceJob, childJobs, cancellationToken);
            }

            return (serviceJob.Id, serviceJob.ImportService);
        }
        finally
        {
            _serviceSemaphoreSlim.Release();
        }
    }

    private async Task EnqueueChildJobAsync(IImportServiceJob parentJob, IImportServiceJob childJob, CancellationToken cancellationToken)
    {
        await _jobExecuteManager.EnqueueChild<IHagfireServiceJobManager, IImportServiceJob>(childJob,
                            (jobManager, job) =>
                                    jobManager.Execute(job.Id,
                                    job.ImportService.Name,
                                    false,
                                    parentJob.Id,
                                    null,
                                    cancellationToken,
                                    null),
                                _aggregateServerSettings,
                                parentJob.Id.ToString(),
                                parentJob,
                                childJob.JobExecuteOptions,
                                null,
                                _childJobEnrichers,
                                cancellationToken);
    }

    private async Task EnqueueCoreServiceJobAsync(IImportServiceJob serviceJob, IEnumerable<IImportServiceJob> childJobs, CancellationToken cancellationToken)
    {
        await _jobExecuteManager.Enqueue<IHagfireServiceJobManager, IImportServiceJob>(serviceJob,
                            childJobs,
                            (jobManager, job) =>
                                    jobManager.Execute(job.Id,
                                    job.ImportService.Name,
                                    job.IsAggregate,
                                    job.ParentId,
                                    null,
                                    cancellationToken,
                                    null),
                                _aggregateServerSettings,
                                serviceJob.JobExecuteOptions,
                                null,
                                _childJobEnrichers,
                                cancellationToken);
    }

    private async Task<IShopModel> GetShopModelAsync(IShopImportSettings shopImportSettings, CancellationToken cancellationToken)
    {
        var shopModel = await _shopDataService.GetShopModelAsync(shopImportSettings, cancellationToken);
        
        if (!ShopModels.Any(s => s.Name == shopModel.Name && s.Type == shopModel.Type))
        {
            ShopModels.Add(shopModel);

            var waitingCategories = _waitingShopCategories.Where(x => x.ShopId == shopModel.Id).ToList();
            waitingCategories.ForEach(x =>
            {
                if (!shopModel.RootCategories.Any(c=>c.ItemId == x.ItemId))
                    shopModel.RootCategories.Add(x.ToProductShopCategoryModel());

                _waitingShopCategories.Remove(x);
            });
        }

        return shopModel;
    }

    private IImportServiceJob CreateImportServiceJob(string name, IShopImportSettings shopImportSettings, IShopModel source)
    {
        var importServiceSettings = shopImportSettings.GetImportService()
            ?? throw new InvalidOperationException($"Import settings {name} does not contain the import service settings");

        var serviceFactory = _shopServiceFactories.FirstOrDefault(f => f.ServiceImplementationType.Name == importServiceSettings.ImplementationTypeName)
            ?? throw new InvalidOperationException($"Service type {importServiceSettings.ImplementationTypeName} not found.");

        return _importServiceJobFactory.CreateServiceJob<IShopModel, IProductShopCategory>(serviceFactory, shopImportSettings, name, source, shopImportSettings.IsAggregate);
    }

    private void SubscribeServiceToFailedHandler(IImportServiceJob importServiceJob)
    {
        async Task serviceConnectedEventHandler(object sender, ConnectedAsyncEventArgs args)
            => await ImportServiceConnectedAsync(sender, args, importServiceJob.Id);

        if (_serviceConnectedHandlers.TryAdd(importServiceJob.Id, serviceConnectedEventHandler))
            importServiceJob.ImportService.ConnectedAsync += serviceConnectedEventHandler;
    }

    private async Task ImportServiceConnectedAsync(object sender, ConnectedAsyncEventArgs eventArgs, Guid serviceJobId)
    {
        if (sender is not IImportService importService)
            throw new InvalidOperationException($"Invalid sender type {sender.GetType().Name}. Sender must be assignable to {nameof(IImportService)}");

        if(!eventArgs.Connected && _allServiceJobs.TryGetValue(serviceJobId, out var importServiceJob))
        {
            var childJobs = GetChildJobs(importServiceJob.Id);

            if (eventArgs.Exception != null)
                    await _jobExecuteManager.StopWithFailedState(importServiceJob.Id.ToString(),
                        childJobs.Select(j => j.Value.Id.ToString()),
                        eventArgs.Exception,
                        _aggregateServerSettings,
                        CancellationToken.None);
        }
    }

    private void UnsubscribeServiceFromConnectedHandler(IImportServiceJob serviceJob)
    {
        if (_serviceConnectedHandlers.TryGetValue(serviceJob.Id, out var childConnectedEventHandler))
        {
            serviceJob.ImportService.ConnectedAsync -= childConnectedEventHandler;

            _serviceConnectedHandlers.TryRemove(serviceJob.Id, out var removedEventHandler);            
        }
    }

    Task<(Guid guid, IImportService service)> IServiceManager.AddImportService(string name, IImportSettings importSettings,
        CancellationToken cancellationToken)
    {
        if (importSettings is not IShopImportSettings shopImportSettings)
            throw new InvalidOperationException($"Invalid import settings type {importSettings.GetType().Name}");

        return AddShopImportService(name, shopImportSettings, cancellationToken);
    }

    public (bool connecting, IImportService service, Task? startTask) StartService(Guid guid, CancellationToken cancellationToken = default)
    {
        var serviceJob = GetCoreServiceJob(guid);
        var connecting = GetCoreServiceJobIsProcessing(guid);
        
        var startTask = !connecting && !serviceJob.ImportService.Connected 
            ? StartService(serviceJob, cancellationToken) 
            : Task.CompletedTask;

        return (connecting, serviceJob.ImportService, Task.Run(() => startTask, cancellationToken));
    }

    private async Task StartService(IImportServiceJob serviceJob, CancellationToken cancellationToken)
    {
        var existingChildJobs = GetChildJobs(serviceJob.Id);

        var childJobs = (existingChildJobs.Count == 0) 
            ? [.. await serviceJob.GetСhildJobs()]
            : existingChildJobs.Values.ToList();        

        await _jobExecuteManager.Execute<IHagfireServiceJobManager, IImportServiceJob>(serviceJob.Id.ToString(),
            childJobs.Select(x=>x.Id.ToString()),
            _aggregateServerSettings,
            serviceJob.JobExecuteOptions,
            cancellationToken);
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
    
    private bool GetCoreServiceJobIsProcessing(Guid guid)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var childJobStorage = scope.ServiceProvider.GetRequiredService<IAggregateJobStorage>();

        var jobEntry = childJobStorage.GetJobEntryByExecutionId(guid.ToString());
        return jobEntry?.Status == JobStatus.Processing;
    }

    private IImportServiceJob GetCoreServiceJob(Guid guid)
    {
        return !_coreServiceJobs.TryGetValue(guid, out var serviceJob)
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

    public async Task AddShopCategory(ShopCategory shopCategory, CancellationToken cancellationToken)
    {
        await _serviceSemaphoreSlim.WaitAsync(cancellationToken);

        try
        {
            var shop = ShopModels.OfType<IProductShopModel>().FirstOrDefault(s => s.Id == shopCategory.ShopId);

            if(shop == null)
            {
                _waitingShopCategories.Add(shopCategory);
                return;
            }

            if (shop.RootCategories.Any() && !await CheckForRootCategoryInAncestor(shopCategory.Id, shop.RootCategories, cancellationToken))
            {
                _waitingShopCategories.Add(shopCategory);
                return;
            }

            var productShopCategory = shopCategory.ToProductShopCategoryModel();

            if (_coreServiceJobs.FirstOrDefault(s => s.Value.SourceId == shop.Id && s.Value.IsAggregate
                     && s.Value is ISourceItemListenerJob<IProductShopCategory>).Value
                                is ISourceItemListenerJob<IProductShopCategory> shopCategoryListenerJob)
            {
                var newServiceJob = shopCategoryListenerJob.AddSource(productShopCategory);

                _allServiceJobs.TryAdd(newServiceJob.Id, newServiceJob);

                try
                {
                    await EnqueueChildJobAsync((shopCategoryListenerJob as IImportServiceJob)!, newServiceJob, cancellationToken);
                }
                catch
                {
                    _allServiceJobs.TryRemove(newServiceJob.Id, out var _);
                    throw;
                }
            }
            else if (_coreServiceJobs.FirstOrDefault(s => s.Value.SourceId == shop.Id && !s.Value.IsAggregate
                     && s.Value.ImportService is IListener<IProductShopCategory>).Value?.ImportService
                                 is IListener<IProductShopCategory> shopCategoryListener)
            {
                await shopCategoryListener.On(productShopCategory, cancellationToken);
            }            
        }
        finally
        {
            ReleaseSemaphoreIfNeed(_serviceSemaphoreSlim);
        }
    }

    private async Task<bool> CheckForRootCategoryInAncestor(int id, IEnumerable<IProductShopCategory> rootCategories, CancellationToken cancellationToken)
    {
        var hasAncestorInRootCategories = false;

        foreach (var rootCategory in rootCategories)
        {
            if ((await _shopDataService.CheckCategoryForItemAncestor(id, rootCategory.ItemId, cancellationToken)) == true)
            {
                hasAncestorInRootCategories = true;
                break;
            }
        }

        return hasAncestorInRootCategories;
    }

    async Task IHagfireServiceJobManager.Execute(Guid id, 
        string displayName, 
        bool isAggregate = false, 
        Guid? parentId = null,
        IJobCancellationToken? jobCancellationToken = null,
        CancellationToken cancellationToken = default,
        PerformContext? performContext = null)
    {
        var retryPolicy = Policy
        .HandleResult<(bool loaded, IImportServiceJob? serviceJob)>((result) => !result.loaded || result.serviceJob is null)
        .WaitAndRetryAsync(
            retryCount: 5,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(10 * (retryAttempt+1)));

        var (loaded, serviceJob) = await retryPolicy.ExecuteAsync(() => LoadServiceJobAsync(id, parentId, displayName, cancellationToken));       
        
        if(!loaded)
            throw new NotLoadedException($"Service {displayName} job {id} has not loaded.");
        
        using var linkedTokenSource =  CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, 
            jobCancellationToken?.ShutdownToken ?? default,
            performContext?.CancellationToken.ShutdownToken ?? default);

        try
        {
            SubscribeServiceToFailedHandler(serviceJob);

            await StartServiceAsync(serviceJob.ImportService, linkedTokenSource.Token);
        }        
        finally
        {
            if (jobCancellationToken?.ShutdownToken.IsCancellationRequested == true)            
                await RemoveJobWithChildrenAsync(serviceJob, !cancellationToken.IsCancellationRequested ? cancellationToken : default);
            else 
                UnsubscribeServiceFromConnectedHandler(serviceJob);
        }
    }

    private async Task<(bool loaded, IImportServiceJob? serviceJob)> LoadServiceJobAsync(Guid id, Guid? parentId, string displayName, CancellationToken cancellationToken)
    {
        if (!_allServiceJobs.TryGetValue(id, out var serviceJob) ||
            (parentId is not null && !_allServiceJobs.TryGetValue(parentId.Value, out var _)))
        {
            using var scope = serviceScopeFactory.CreateScope();
            var childJobStorage = scope.ServiceProvider.GetRequiredService<IAggregateJobStorage>();

            var jobEntry = await childJobStorage.GetJobEntryByExecutionIdAsync(id.ToString(), cancellationToken);

            if(jobEntry?.Status == JobStatus.Deleted)
               throw new JobDeletedException($"Job {id} {displayName} is deleted.");

            return (false, default);
        }

        return (true, serviceJob);
    }

    private async Task RemoveJobWithChildrenAsync(IImportServiceJob deletedServiceJob, CancellationToken cancellationToken = default)
    {
        await _jobExecuteManager.Delete(deletedServiceJob.Id.ToString(), cancellationToken);

        UnsubscribeServiceFromConnectedHandler(deletedServiceJob);

        var childJobs = GetChildJobs(deletedServiceJob.Id);
        foreach (var childJob in childJobs)
        {
            UnsubscribeServiceFromConnectedHandler(childJob.Value);

            await _jobExecuteManager.Delete(childJob.Value.Id.ToString(), cancellationToken);
        }
    }

    private IReadOnlyDictionary<Guid, IImportServiceJob> GetChildJobs(Guid parentId)
    {
        return _allServiceJobs.Where(j => j.Value.ParentId == parentId).ToDictionary();
    }

    private static void ReleaseSemaphoreIfNeed(SemaphoreSlim semaphoreSlim)
    {
        if (semaphoreSlim.CurrentCount < 1)
        {
            semaphoreSlim.Release();
        }
    }

    public ValueTask DisposeAsync()
    {
        foreach (var serviceJob in _allServiceJobs.Values)
            UnsubscribeServiceFromConnectedHandler(serviceJob);

        ReleaseSemaphoreIfNeed(_serviceSemaphoreSlim);
        _serviceSemaphoreSlim.Dispose();
        
        return ValueTask.CompletedTask;
    }
}