namespace Alchemist.Import.Settings.Interfaces;

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

    public ICategoryUrl[]? RootCategories { get; set; }
}
