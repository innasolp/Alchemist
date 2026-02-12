using Alchemist.Product.Data;
using Mediator.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;
using ShopSettings.UnitOfWork;
using ShopSettingType = Alchemist.Product.Data.ShopSettingType;

namespace ShopSettings.Infrastructure.EF;

public sealed class ShopSettingsRepository(AlchemyContext context) : IShopSettingsRepository
{
    private readonly AlchemyContext _context = context;

    public Task<List<Alchemist.Product.Data.ShopSettings>> GetAllParentShopSettings(CancellationToken cancellationToken = default)
    {
        return _context.ShopSettings.Where(s => s.ParentSettingsId == null).ToListAsync(cancellationToken);
    }

    public Task<List<Alchemist.Product.Data.ShopSettings>> GetChildSettings(int parentSettingsId, CancellationToken cancellationToken = default)
    {
        return _context.ShopSettings.Where(s => s.ParentSettingsId == parentSettingsId).ToListAsync(cancellationToken);
    }

    public Task<Alchemist.Product.Data.ShopSettings?> GetShopSettingsByShopId(int shopId, ShopSettingType settingType, CancellationToken cancellationToken = default)
    {
        return _context.ShopSettings.FirstOrDefaultAsync(s => s.ShopId == shopId && s.Type == settingType && s.IsActual != false, cancellationToken);
    }

    public async Task<Alchemist.Product.Data.ShopSettings?> SaveShopSettings(Alchemist.Product.Data.ShopSettings shopSettings, CancellationToken cancellationToken = default)
    {
        await SetShopSettingsActuality(shopSettings.ShopId, shopSettings.Type, shopSettings.Id, cancellationToken);

        var result = await AddOrUpdateShopSettings(shopSettings, cancellationToken);       

        return result;
    }

    private  Task<int> SetShopSettingsActuality(int shopId, ShopSettingType shopSettingType, int actualId, CancellationToken cancellationToken = default)
    {
        return  _context.ShopSettings.Where(s => s.ShopId == shopId
            && s.Id != actualId
            && s.Type == shopSettingType && s.Type != ShopSettingType.Service
            && s.IsActual != false)
            .ExecuteUpdateAsync(settings =>
                settings.SetProperty(s => s.IsActual, s => false),
            cancellationToken);
    }

    private async Task<Alchemist.Product.Data.ShopSettings> AddOrUpdateShopSettings(Alchemist.Product.Data.ShopSettings shopSettings, CancellationToken cancellationToken = default)
    {
        shopSettings.IsActual = true;

        var result = await _context.ShopSettings.Where(s => s.Id == shopSettings.Id)
           .ExecuteUpdateAsync(settings =>
                settings.SetProperty(s => s.IsActual, s => shopSettings.IsActual)
                        .SetProperty(s => s.JsonValue, s => shopSettings.JsonValue)
                        .SetProperty(s => s.ParentSettingsId, s => shopSettings.ParentSettingsId)
                        .SetProperty(s => s.Type, s => shopSettings.Type)
                        .SetProperty(s => s.ShopId, s => shopSettings.ShopId)
                        .SetProperty(s => s.Name, s => shopSettings.Name),
            cancellationToken);

        if (result > 0)
            return shopSettings;

        var added = await _context.Create(shopSettings, cancellationToken);       

        return added;
    }

    public async Task<(Alchemist.Product.Data.ShopSettings, IEnumerable<Alchemist.Product.Data.ShopSettings>)> SaveShopSettingsWithChildren(Alchemist.Product.Data.ShopSettings parentShopSettings,
        IEnumerable<Alchemist.Product.Data.ShopSettings> childrenSettings, CancellationToken cancellationToken = default)
    {
        await SetShopSettingsActuality(parentShopSettings.ShopId, parentShopSettings.Type, parentShopSettings.Id, cancellationToken);

        var shopSettings = await AddOrUpdateShopSettings(parentShopSettings, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        var handledServices = new List<Alchemist.Product.Data.ShopSettings> ();
        foreach (var service in childrenSettings)
        {
            service.ParentSettingsId = shopSettings.Id;
            service.Type = ShopSettingType.Service;
            handledServices.Add(await AddOrUpdateShopSettings(service, cancellationToken));
        }

        return (shopSettings, handledServices);
    }
}