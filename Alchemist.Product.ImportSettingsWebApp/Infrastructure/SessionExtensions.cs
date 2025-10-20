using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.Interfaces;
using System.Text.Json;

namespace Alchemist.Product.ImportSettingsWebApp.Infrastructure;

public static class SessionExtensions
{
    public static void SetImportSettingtoSession(this ISession session, ShopImportSettingsModel shopImportSettings)
    {
        session.SetInt32(SessionKeys.ShopSettingsTypeKey, (int)shopImportSettings.ShopSettingType);

        var bytes = JsonSerializer.SerializeToUtf8Bytes(shopImportSettings, shopImportSettings.GetType());

        session.Set(SessionKeys.ShopImportSettingsKey, bytes);
    }

    public static async Task<ShopImportSettingsModel?> GetShopImportSettingsFromSessionAsync(this ISession session)
    {
        var shopSettingType = session.GetInt32(SessionKeys.ShopSettingsTypeKey);

        if(shopSettingType == null) return null;

        if (session.TryGetValue(SessionKeys.ShopImportSettingsKey, out var bytes))
        {
            using var stream = new MemoryStream(bytes);
            return (ShopSettingType)shopSettingType == ShopSettingType.Product
                 ? await JsonSerializer.DeserializeAsync<ProductShopImportSettingsModel>(stream)
                 : await JsonSerializer.DeserializeAsync<CategoryShopImportSettingsModel>(stream);
        }

        return null;
    }
}
