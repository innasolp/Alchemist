using Alchemist.DataService.Interfaces;
using Alchemist.Product.Data;
using Alchemist.Product.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Alchemist.Settings.Data.Repository;

public class SettingsRepository : ISettingsRepository
{
    protected AlchemyContext Context { get; }

    public SettingsRepository(AlchemyContext context) => Context = context;

    public async Task<IShopSettings?> GetShopSettings(int shopId, ShopSettingType settingType, CancellationToken cancellationToken = default)
    {
        return await Context.ShopSettings.FirstOrDefaultAsync(s => s.ShopId == shopId && s.Type == settingType && s.IsActual != false, cancellationToken);
    }

    public async Task<IShopSettings?> GetShopSettings(int id, CancellationToken cancellationToken = default)
    {
        return await Context.ShopSettings.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    private async Task<int> SetShopSettingsActuality(int shopId, ShopSettingType shopSettingType, int actualId, CancellationToken cancellationToken = default)
    {
        var result = await Context.ShopSettings.Where(s => s.ShopId == shopId
            && s.Id != actualId
            && s.Type == shopSettingType && s.Type != ShopSettingType.Service
            && s.IsActual != false)
            .ExecuteUpdateAsync(settings =>
                settings.SetProperty(s => s.IsActual, s => false),
            cancellationToken);

        // ExecuteUpdateAsync applies changes directly; SaveChangesAsync is not required,
        // but keep a consistent pattern if additional change tracking exists.
        await Context.SaveChangesAsync(cancellationToken);

        return result;
    }

    private async Task<IShopSettings> AddOrUpdateShopSettings(IShopSettings shopSettings, CancellationToken cancellationToken = default)
    {
        shopSettings.IsActual = true;

        var result = await Context.ShopSettings.Where(s => s.Id == shopSettings.Id)
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

        var entity = shopSettings.To<ShopSettings>();
        Context.ShopSettings.Add(entity);

        await Context.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task<IShopSettings?> SaveShopSettings(IShopSettings shopSettings, CancellationToken cancellationToken = default)
    {
        await using var dbContextTransaction = await Context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await SetShopSettingsActuality(shopSettings.ShopId, shopSettings.Type, shopSettings.Id, cancellationToken);

            var result = await AddOrUpdateShopSettings(shopSettings, cancellationToken);

            await dbContextTransaction.CommitAsync(cancellationToken);

            return result;
        }
        catch
        {
            await dbContextTransaction.RollbackAsync(cancellationToken);
            return default;
        }
    }

    public async Task<bool> UpdateShopSettings(IShopSettings shopSettings, CancellationToken cancellationToken = default)
    {
        await Context.ShopSettings.Where(s => s.Id == shopSettings.Id)
            .ExecuteUpdateAsync(settings => settings.SetProperty(s => s.IsActual, s => shopSettings.IsActual)
                .SetProperty(s => s.JsonValue, s => shopSettings.JsonValue)
                .SetProperty(s => s.ParentSettingsId, s => shopSettings.ParentSettingsId)
                .SetProperty(s => s.Type, s => shopSettings.Type)
                .SetProperty(s => s.ShopId, s => shopSettings.ShopId),
            cancellationToken);

        var result = await Context.SaveChangesAsync(cancellationToken);

        return result > 0;
    }

    public async Task<List<IShopSettings>> GetChildSettings(int parentSettingsId, CancellationToken cancellationToken = default)
    {
        var list = await Context.ShopSettings.Where(s => s.ParentSettingsId == parentSettingsId).ToListAsync(cancellationToken);
        return [.. list.OfType<IShopSettings>()];
    }

    public async Task<List<IShopSettings>> SaveShopSettings(IShopSettings parentShopSettings, IEnumerable<IShopSettings> childrenSettings, CancellationToken cancellationToken = default)
    {
        await using var dbContextTransaction = await Context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await SetShopSettingsActuality(parentShopSettings.ShopId, parentShopSettings.Type, parentShopSettings.Id, cancellationToken);

            var shopSettings = await AddOrUpdateShopSettings(parentShopSettings, cancellationToken);

            var handledServices = new List<IShopSettings> { shopSettings };
            foreach (var service in childrenSettings)
            {
                service.ParentSettingsId = shopSettings.Id;
                service.Type = ShopSettingType.Service;
                handledServices.Add(await AddOrUpdateShopSettings(service, cancellationToken));
            }

            await dbContextTransaction.CommitAsync(cancellationToken);

            return handledServices;
        }
        catch
        {
            await dbContextTransaction.RollbackAsync(cancellationToken);
            return [];
        }
    }

    public async Task<IShopSettings?> GetShopSettings(string shopSettingsName, CancellationToken cancellationToken = default)
    {
        return await Context.ShopSettings.FirstOrDefaultAsync(s => s.Name == shopSettingsName, cancellationToken);
    }

    public async Task<List<IShopSettings>> GetAllParentShopSettings(CancellationToken cancellationToken = default)
    {
        var list = await Context.ShopSettings.Where(s => s.ParentSettingsId == null).ToListAsync(cancellationToken);
        return [.. list.OfType<IShopSettings>()];
    }
}