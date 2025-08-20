using System.Collections;

namespace Alchemist.Import.Settings.Interfaces;

public interface IShopImportSettings: ISettings
{        
    IImportServiceSettings ImportService { get; set; }

    IImportServiceSettings? RequestHeaders { get; set; }

    IImportServiceSettings WebLoader { get; set; }

    IImportServiceSettings? BrowserDataLoader { get; set; }

    IImportServiceSettings? BrowserLauncher { get; set; }

    public bool? Perfomance { get; set; }

    IList Services { get; }

    string ShopName { get; set; }

    string ShopUrl { get; set; }
}
