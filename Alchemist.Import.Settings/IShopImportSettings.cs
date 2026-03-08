using Import.Settings.Interfaces;

namespace Alchemist.Import.Settings;

public interface IShopImportSettings : IImportSettings
{
    public bool IsAggregate { get; set; }

    string ShopName { get; set; }

    string ShopUrl { get; set; }
}