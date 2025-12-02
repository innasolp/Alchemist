using Import.Settings.Interfaces;

namespace Alchemist.Import.Settings.Product;

public interface ICategoryUrl
{
    int Item { get; set; }

    string Url { get; set; }
}

public interface IProductShopImportSettings : IShopImportSettings
{
    public string? ProductUrlFormat { get; set; }

    public string? CategoryUrlFormat { get; set; }

    public int? PageProductCount { get; set; }

    public IEnumerable<ICategoryUrl>? RootCategories { get; set; }
}
