using Import.Settings.Interfaces;

namespace Alchemist.Import.Settings;

public interface IShopImportSettings : IImportSettings
{
    public bool? Perfomance { get; set; }

    string ShopName { get; set; }

    string ShopUrl { get; set; }
}