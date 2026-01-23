using Alchemist.Import.Settings;
using Alchemist.Import.Settings.Extensions;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Service.Commands.Models;
using Import.Settings.Interfaces;
using Shop.Interfaces;
using System.Collections.Concurrent;

namespace Import.Service.Commands;

internal class ShopImportServiceRepository(IEnumerable<IImportServiceFactory> shopServiceFactories, IShopDataService shopDataService) : IServiceRepository
{
    private readonly ConcurrentDictionary<Guid, ServiceItem> _services = new();    

    private readonly IEnumerable<IImportServiceFactory> _shopServiceFactories = shopServiceFactories;

    private readonly IShopDataService _shopDataService = shopDataService;

    private readonly SemaphoreSlim _addServiceSemaphoreSlim = new(1);

    private List<IImportSource> ShopModels { get; } = [];

    public IReadOnlyDictionary<Guid, ServiceItem> Services => _services;

    IReadOnlyList<IImportSource> IServiceRepository.ShopModels => ShopModels;

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
}