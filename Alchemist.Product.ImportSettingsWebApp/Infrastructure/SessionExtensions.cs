using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.Interfaces;
using System.Text.Json;

namespace Alchemist.Product.ImportSettingsWebApp.Infrastructure;

internal static class SessionExtensions
{
    internal static void SetImportSettingtoSession(this ISession session, ShopImportSettingsModel shopImportSettings)
    {
        session.SetInt32(SessionKeys.ShopSettingsTypeKey, (int)shopImportSettings.ShopSettingType);

        var bytes = JsonSerializer.SerializeToUtf8Bytes(shopImportSettings);

        session.Set(SessionKeys.ShopImportSettingsKey, bytes);
    }

    internal static async Task<ShopImportSettingsModel?> GetShopImportSettingsFromSessionAsync(this ISession session)
    {
        var shopSettingType = session.GetInt32(SessionKeys.ShopSettingsTypeKey) ??
            throw new InvalidOperationException("Session does not contains shopsettingstype.");

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
