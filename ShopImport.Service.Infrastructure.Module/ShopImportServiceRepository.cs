using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Entities;
using Autofac.Core;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Service.Commands.Models;
using Import.Service.Infrastructure;
using Import.Settings.Interfaces;
using Shop.Interfaces;
using ShopImport.Service.Infrastructure.Module.Models;
using System.Collections.Concurrent;

namespace ShopImport.Service.Infrastructure.Module;

internal class ShopImportServiceRepository(IEnumerable<IImportServiceFactory> shopServiceFactories, IShopDataService shopDataService) : IShopImportServiceRepository
{
    private readonly ConcurrentDictionary<Guid, ServiceItem> _services = new();    

    private readonly IEnumerable<IImportServiceFactory> _shopServiceFactories = shopServiceFactories;

    private readonly IShopDataService _shopDataService = shopDataService;

    private readonly SemaphoreSlim _addServiceSemaphoreSlim = new(1);

    private List<IImportSource> ShopModels { get; } = [];

    public async Task<(Guid guid, IImportService service)> AddShopImportService(string name, IShopImportSettings shopImportSettings,
        CancellationToken cancellationToken = default)
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

    Task<(Guid guid, IImportService service)> IServiceRepository.AddImportService(string name, IImportSettings importSettings, 
        CancellationToken cancellationToken)
    {
        if(importSettings is not IShopImportSettings shopImportSettings)
            throw new InvalidOperationException($"Invalid import settings type {importSettings.GetType().Name}");        

        return AddShopImportService(name, shopImportSettings, cancellationToken);
    }

    public async Task ConsumeItem<T>(T item, Func<T, IShopModel, bool> isConsumerSource, CancellationToken cancellationToken = default)
    {
        var shops = ShopModels.OfType<IShopModel>().Where(s =>isConsumerSource(item, s));

        var consumeServicesItems = _services.Where(s => s.Value.Service is IListener<T> && shops.Any(shop => s.Value.SourceId == shop.Id));
        
        var tasks = consumeServicesItems.Select(s => s.Value.Service).OfType<IListener<T>>().Select(s => s.On(item, cancellationToken));

        await Task.WhenAll(tasks);
    }

    public (IImportService service, Task startTask) StartServiceTask(Guid guid, CancellationToken cancellationToken)
    {
        if (!_services.TryGetValue(guid, out var serviceItem))
            throw new InvalidOperationException($"Service with id {guid} not found.");

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, serviceItem.InnerTokenSource.Token);

        var startTask = StartAndDisposeAsync(serviceItem.Service, linkedCts);
        return (serviceItem.Service, startTask);
    }

    static async Task StartAndDisposeAsync(IImportService service, CancellationTokenSource linkedCts)
    {
        try
        {
            await service.Start(linkedCts.Token);
        }
        finally
        {
            linkedCts.Dispose();
        }
    }

    public (IImportService service, Task startTask) StopServiceTask(Guid guid, CancellationToken cancellationToken)
    {
        if (!_services.TryGetValue(guid, out var serviceItem))
            throw new InvalidOperationException($"Service with id {guid} not found.");

        return (serviceItem.Service, StopServiceAsync(serviceItem, cancellationToken));
    }

    private static async Task StopServiceAsync(ServiceItem serviceItem, CancellationToken cancellationToken)
    {
        try
        {
            await serviceItem.InnerTokenSource.CancelAsync();
            await serviceItem.Service.Stop(cancellationToken);
        }
        finally
        {
            serviceItem.InnerTokenSource.Dispose();
        }
    }

    public IEnumerable<(Guid guid, IImportService service, Task stopTask)> StopAllServicesTask(CancellationToken cancellationToken)
    {
        return _services.Select(si => (si.Key, si.Value.Service, StopServiceAsync(si.Value, cancellationToken)));
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