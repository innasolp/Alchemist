using Alchemist.Product.Interfaces;

namespace Alchemist.Import.Settings.Interfaces;

public interface IShopImportData
{
    IShop Shop { get; }

    IShopUrl ShopUrl { get; }

    IProductShopImportSettings ProductShopImportSettings { get; }

    ICategoryShopImportSettings? CategoryShopImportSettings { get; }
}
