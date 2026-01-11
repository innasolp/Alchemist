using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Import.Settings.Product;

public interface ICategoryUrl
{
    int Item { get; set; }

    string Url { get; set; }
}

public interface IProductShopImportSettings : IShopImportSettings
{
    string? ProductUrlFormat { get; set; }

    PathFormatType ProductUrlFormatType { get; set; }

    string? CategoryUrlFormat { get; set; }

    PathFormatType CategoryUrlFormatType { get; set; }

    int? PageProductCount { get; set; }

    IEnumerable<ICategoryUrl>? RootCategories { get; set; }
}