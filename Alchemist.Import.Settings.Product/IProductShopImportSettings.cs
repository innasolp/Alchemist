namespace Alchemist.Import.Settings.Product;

public interface ICategoryUrl
{
    int Item { get; set; }

    string Url { get; set; }
}

public interface IProductShopImportSettings : IShopImportSettings
{
    string? ProductUrlFormat { get; set; }

    UrlFormatType ProductUrlFormatType { get; set; }

    string? CategoryUrlFormat { get; set; }

    UrlFormatType CategoryUrlFormatType { get; set; }

    int? PageProductCount { get; set; }

    IEnumerable<ICategoryUrl>? RootCategories { get; set; }
}