using Alchemist.DataService.Interfaces;
using Alchemist.Product.Data;
using Alchemist.Product.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Alchemist.Settings.Data.Repository;

public class SettingsRepository(AlchemyContext context) : ISettingsRepository
{
    protected AlchemyContext Context { get; set; } = context;

    public async Task<IShopSettings?> GetShopSettings(int shopId, ShopSettingType settingType)
    {
        return await Context.ShopSettings.FirstOrDefaultAsync(s => s.ShopId == shopId && s.Type == settingType && s.IsActual != false);
    }

    public async Task<IShopSettings?> GetShopSettings(int id)
    {
        return await Context.ShopSettings.FirstOrDefaultAsync(s => s.Id == id);
    }

    private async Task<int> SetShopSettingsActuality(int shopId, ShopSettingType shopSettingType, int actualId)
    {
        var result = await Context.ShopSettings.Where(s => s.ShopId == shopId
        && s.Id != actualId
        && s.Type == shopSettingType && s.Type != ShopSettingType.Service
        && s.IsActual != false).ExecuteUpdateAsync(settings =>
            settings.SetProperty(s => s.IsActual, s => false));

        await Context.SaveChangesAsync();

        return result;
    }

    private async Task<IShopSettings> AddOrUpdateShopSettings(IShopSettings shopSettings)
    {
        shopSettings.IsActual = true;
        var result = await Context.ShopSettings.Where(s => s.Id == shopSettings.Id)
           .ExecuteUpdateAsync(settings =>
            settings.SetProperty(s => s.IsActual, s => shopSettings.IsActual)
            .SetProperty(s => s.JsonValue, s => shopSettings.JsonValue)
            .SetProperty(s => s.ParentSettingsId, s => shopSettings.ParentSettingsId)
            .SetProperty(s => s.Type, s => shopSettings.Type)
            .SetProperty(s => s.ShopId, s => shopSettings.ShopId)
           );

        if (result > 0) return shopSettings;

        var entity = shopSettings.To<ShopSettings>();
        var added = Context.ShopSettings.Add(entity);

        await Context.SaveChangesAsync();

        return await Task.FromResult(entity);
    }

    public async Task<IShopSettings?> SaveShopSettings(IShopSettings shopSettings)
    {
        using var dbContextTransaction = context.Database.BeginTransaction();
        try
        {
            await SetShopSettingsActuality(shopSettings.ShopId, shopSettings.Type, shopSettings.Id);

            var result = await AddOrUpdateShopSettings(shopSettings);            

            dbContextTransaction.Commit();

            return await Task.FromResult(result);
        }
        catch
        {
            dbContextTransaction.Rollback();
            return await Task.FromResult(default(IShopSettings));
        }
    }

    public async Task<bool> UpdateShopSettings(IShopSettings shopSettings)
    {
        await Context.ShopSettings.Where(s => s.Id == shopSettings.Id)
            .ExecuteUpdateAsync(settings => settings.SetProperty(s => s.IsActual, s => shopSettings.IsActual)
            .SetProperty(s => s.JsonValue, s => shopSettings.JsonValue)
            .SetProperty(s => s.ParentSettingsId, s => shopSettings.ParentSettingsId)
            .SetProperty(s => s.Type, s => shopSettings.Type)
            .SetProperty(s => s.ShopId, s => shopSettings.ShopId));

        var result = await Context.SaveChangesAsync();

        return await Task.FromResult(result > 0);
    }

    public async Task<List<IShopSettings>> GetChildSettings(int parentSettingsId)
    {
        return await Context.ShopSettings.Where(s => s.ParentSettingsId == parentSettingsId).ToListAsync<IShopSettings>();
    }

    public async Task<List<IShopSettings>> SaveShopSettings(IShopSettings parentShopSettings, IEnumerable<IShopSettings> childrenSettings)
    {
        using var dbContextTransaction = context.Database.BeginTransaction();
        try
        {
            await SetShopSettingsActuality(parentShopSettings.ShopId, parentShopSettings.Type, parentShopSettings.Id);

            var shopSettings = await AddOrUpdateShopSettings(parentShopSettings);

            var handledServices = new List<IShopSettings> { shopSettings };
            foreach (var service in childrenSettings)
            {
                service.ParentSettingsId = shopSettings.Id;
                service.Type = ShopSettingType.Service;
                handledServices.Add(await AddOrUpdateShopSettings(service));
            }            

            dbContextTransaction.Commit();

            return await Task.FromResult(handledServices);
        }
        catch
        {
            dbContextTransaction.Rollback();
            return await Task.FromResult(new List<IShopSettings>());
        }
    }
}
