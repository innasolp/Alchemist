using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Product.Import.Model;

public interface IShopServicesSettingsModel : IShopSettingsModel, IShopImportSettings
{
    new IServiceSettingsModel RequestHeaders { get; }

    new IServiceSettingsModel ImportService { get; }

    new IServiceSettingsModel BrowserDataLoader { get; }

    new IServiceSettingsModel WebLoader { get; }    

    string? FileName { get; set; }
}
