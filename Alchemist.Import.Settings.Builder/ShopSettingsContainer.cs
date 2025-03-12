using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Import.Settings.Builders;

public class ShopSettingsContainer(IShop shop, IProductShopImportSettings productShopImportSettings, ICategoryShopImportSettings? categoryShopImportSettings)
{
    public IShop Shop { get; } = shop;
    public IProductShopImportSettings ProductShopImportSettings { get; } = productShopImportSettings;
    public ICategoryShopImportSettings? CategoryShopImportSettings { get; } = categoryShopImportSettings;
}
