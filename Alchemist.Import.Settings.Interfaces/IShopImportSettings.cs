using System.Collections;

namespace Alchemist.Import.Settings.Interfaces;

public interface IShopImportSettings: ISettings
{ 
    public bool? Perfomance { get; set; }

    IList Services { get; }

    string ShopName { get; set; }

    string ShopUrl { get; set; }
}
