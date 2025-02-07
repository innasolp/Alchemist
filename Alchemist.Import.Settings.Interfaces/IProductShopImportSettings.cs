namespace Alchemist.Import.Settings.Interfaces;

public interface IProductShopImportSettings : IShopImportSettings
{
    public string? ProductUrl { get; }

    public string? CategoryUrl { get; }

    public int? PageProductCount { get; }
}
