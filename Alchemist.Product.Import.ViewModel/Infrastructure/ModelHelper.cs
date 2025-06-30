using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Product.Import.Model.Infrastructure;

public static class ModelHelper
{
    public static bool IsServiceSettingsPrimary(string name)
    {
        return name == nameof(IShopImportSettings.ImportService)
            || name == nameof(IShopImportSettings.BrowserDataLoader)
            || name == nameof(IShopImportSettings.WebLoader)
            || name == nameof(IShopImportSettings.RequestHeaders);
    }
    




}
