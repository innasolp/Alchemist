using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Entities;
using Hangfire;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Service.Commands.Models;
using Import.Service.Infrastructure;
using Import.Settings.Interfaces;
using LongRunningTask;
using Shop.Interfaces;
using ShopImport.Service.Hangfire.Models;
using ShopImport.Service.Infrastructure.Module.Models;
using System.Collections.Concurrent;

namespace ShopImport.Service.Hangfire;

internal class ShopImportServiceManager(IEnumerable<IImportServiceFactory> shopServiceFactories, IShopDataService shopDataService) : IShopImportServiceManager
{
    private readonly IEnumerable<IImportServiceFactory> _shopServiceFactories = shopServiceFactories;

    private readonly IShopDataService _shopDataService = shopDataService;

    private readonly ConcurrentDictionary<Guid, ServiceItem> _services = new();

    private readonly ConcurrentDictionary<string, ImportServiceJob> _backgroundJobs = new();

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

            var service = CreateImportService(name, shopImportSettings, shopModel);

            var serviceItem = new ServiceItem(Guid.NewGuid(), service, shopModel.Id, new CancellationTokenSource());
            var added = _services.TryAdd(serviceItem.Guid, serviceItem);

            return (serviceItem.Guid, service);
        }
        finally
        {
            _addServiceSemaphoreSlim.Release();
        }
    }

    private IImportService CreateImportService(string name, IShopImportSettings shopImportSettings, IShopModel source)
    {
        var importServiceSettings = shopImportSettings.GetImportService()
            ?? throw new InvalidOperationException($"Import settings {name} does not contain the import service settings");

        var serviceFactory = _shopServiceFactories.FirstOrDefault(f => f.ServiceImplementationType.Name == importServiceSettings.ImplementationTypeName)
            ?? throw new InvalidOperationException($"Service type {importServiceSettings.ImplementationTypeName} not found.");

        return serviceFactory.Create(name, source, shopImportSettings);
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
        if (!_services.TryGetValue(guid, out var serviceItem))
            throw new InvalidOperationException($"Service with id {guid} not found.");

        var startTask = StartServiceTask(serviceItem);

        return (serviceItem.Service, startTask);
    }

    private Task StartServiceTask(ServiceItem importServiceItem)
    {
        if (!string.IsNullOrEmpty(importServiceItem.JobId) && _backgroundJobs.TryGetValue(importServiceItem.JobId, out var job) && job != null)
            throw new InvalidOperationException($"Job for service {importServiceItem.Service.Name} already run.");

        var importServiceJob = new ImportServiceJob(importServiceItem);
        var jobId = BackgroundJob.Enqueue<IBackgroundJobService>((job) => job.Execute(importServiceJob, null));
        _backgroundJobs.TryAdd(jobId, importServiceJob);

        importServiceItem.JobId = jobId;

        return Task.CompletedTask;
    }

    public (IImportService service, Task startTask) StopServiceTask(Guid guid, CancellationToken cancellationToken)
    {
        if (!_services.TryGetValue(guid, out var serviceItem))
            throw new InvalidOperationException($"Service with id {guid} not found.");
        
        return (serviceItem.Service, StopServiceAsync(serviceItem, cancellationToken));
    }

    private void DequeueServiceTask(ServiceItem importServiceItem)
    {
        if (string.IsNullOrEmpty(importServiceItem.JobId) || !_backgroundJobs.TryGetValue(importServiceItem.JobId, out var job) || job == null)
            throw new InvalidOperationException($"Job for service {importServiceItem.Service.Name} not found.");

        BackgroundJob.Delete(importServiceItem.JobId);

        _backgroundJobs.TryRemove(importServiceItem.JobId, out job);
    }


    public IEnumerable<(Guid guid, IImportService service, Task stopTask)> StopAllServicesTask(CancellationToken cancellationToken)
    {
        return _services.Select(si => (si.Key, si.Value.Service, StopServiceAsync(si.Value, cancellationToken)));
    }

    private async Task StopServiceAsync(ServiceItem serviceItem, CancellationToken cancellationToken)
    {
        await serviceItem.Service.Stop(cancellationToken);
        DequeueServiceTask(serviceItem);
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

        if (_services.FirstOrDefault(s => s.Value.SourceId == shop?.Id
                    && s.Value.Service is IListener<IProductShopCategory> shopCategoryListener).Value.Service
            is IListener<IProductShopCategory> shopCategoryListener)
            await shopCategoryListener.On(productShopCategory);
    }
}