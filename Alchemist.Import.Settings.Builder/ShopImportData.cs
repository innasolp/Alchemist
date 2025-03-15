using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Import.Settings.Builders;

public class ShopImportData(IShop shop, IShopUrl shopUrl, IProductShopImportSettings productShopImportSettings, ICategoryShopImportSettings? categoryShopImportSettings)
    : IShopImportData
{
    public IShop Shop { get; } = shop;

    public IShopUrl ShopUrl { get; } = shopUrl;

    public IProductShopImportSettings ProductShopImportSettings { get; } = productShopImportSettings;

    public ICategoryShopImportSettings? CategoryShopImportSettings { get; } = categoryShopImportSettings;
}
