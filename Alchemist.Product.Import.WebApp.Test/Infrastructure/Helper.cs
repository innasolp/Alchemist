using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Product.Import.WebApp.Test.Infrastructure;

internal static class Helper
{
    public static bool IsServiceSettingsPrimary(string name)
    {
        return name == nameof(IShopImportSettings.ImportService)
            || name == nameof(IShopImportSettings.BrowserDataLoader)
            || name == nameof(IShopImportSettings.WebLoader)
            || name == nameof(IShopImportSettings.RequestHeaders);
    }
}
