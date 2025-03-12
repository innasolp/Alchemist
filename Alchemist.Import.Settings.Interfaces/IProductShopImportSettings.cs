namespace Alchemist.Import.Settings.Interfaces;

public interface IProductShopImportSettings : IShopImportSettings
{
    public string? ProductUrl { get; set; }

    public string? CategoryUrl { get; set; }

    public int? PageProductCount { get; set; }
}
