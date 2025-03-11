namespace Alchemist.Import.Settings.Interfaces;

public static class Common
{
    public static string[] BaseServiceNames = [nameof(IShopImportSettings.ImportService),
        nameof(IShopImportSettings.BrowserDataLoader),
        nameof(IShopImportSettings.RequestHeaders),
        nameof(IShopImportSettings.WebLoader)];
}
