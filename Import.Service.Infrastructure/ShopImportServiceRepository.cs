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

    public async Task<(bool, Guid guid, IImportService? service)> TryAddShopImportService(string name, IShopImportSettings shopImportSettings,
        CancellationToken cancellationToken = default)
    {
        await _addServiceSemaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            var shopModel = await _shopDataService.GetShopModelAsync(shopImportSettings, cancellationToken);
            if (!ShopModels.Any(s => s.Name == ((IImportSource)shopModel).Name))
                ShopModels.Add(shopModel);

            if (!TryCreateImportService(name, shopImportSettings, shopModel, out var service) || service is null)
                return (false, Guid.Empty, default);

            var serviceItem = new ServiceItem(Guid.NewGuid(), service, shopModel.Id, new CancellationTokenSource());
            var added = _services.TryAdd(serviceItem.Guid, serviceItem);

            return (added, serviceItem.Guid, service);
        }
        finally
        {
            _addServiceSemaphoreSlim.Release();
        }
    }

    private bool TryCreateImportService(string name, IShopImportSettings shopImportSettings, IShopModel source, out IImportService? service)
    {
        service = default;

        var importServiceSettings = shopImportSettings.GetImportService();
        if (importServiceSettings == null)
            return false;

        var serviceFactory = _shopServiceFactories.FirstOrDefault(f => f.ServiceImplementationType.Name == importServiceSettings.ImplementationTypeName);
        if (serviceFactory == null) return false;

        service = serviceFactory.Create(name, source, shopImportSettings);
        return true;
    }

    Task<(bool, Guid guid, IImportService? service)> IServiceRepository.TryAddImportService(string name, IImportSettings importSettings, 
        CancellationToken cancellationToken = default)
    {
        if(importSettings is not IShopImportSettings shopImportSettings)
            throw new InvalidOperationException($"Invalid import settings type {importSettings.GetType().Name}");        

        return TryAddShopImportService(name, shopImportSettings, cancellationToken);
    }
}