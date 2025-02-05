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

    public async Task<IShopSettings> AddShopSettings(IShopSettings shopSettings)
    {
        await Context.ShopSettings.Where(s => s.ShopId == shopSettings.ShopId && s.Type == shopSettings.Type && s.IsActual != false)
            .ExecuteUpdateAsync(settings => settings.SetProperty(s => s.IsActual, s => false));

        var entity = shopSettings.To<ShopSettings>();
        var added = await Context.ShopSettings.AddAsync(entity);

        await Context.SaveChangesAsync();

        return await Task.FromResult(added.Entity);
    }

    public async Task<bool> UpdateShopSettings(IShopSettings shopSettings)
    {
        await Context.ShopSettings.Where(s => s.Id == shopSettings.Id)
            .ExecuteUpdateAsync(settings => settings.SetProperty(s => s.IsActual, s => shopSettings.IsActual)
            .SetProperty(s=>s.JsonValue, s=>shopSettings.JsonValue)
            .SetProperty(s=>s.ShopId, s=>shopSettings.ShopId));       

        var result = await Context.SaveChangesAsync();

        return await Task.FromResult(result > 0);
    }
}
