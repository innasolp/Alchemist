using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;

namespace Alchemist.DataService.Interfaces;

public interface IProductDataService
{
    Task<ProductType> CreateProductType(ProductType productType);
    Task<ProductType?> FindProductTypeByName(string name);

    Task<PurposeType> CreatePurposeType(PurposeType purposeType);
    Task<PurposeType?> FindPurposeTypeByName(string name);

    Task<Country> CreateCountry(Country country);
    Task<Country?> FindCountryByName(string name);

    Task<Brand> CreateBrand(Brand brand);
    Task<Brand?> FindBrandByName(string name);

    Task<Component> CreateComponent(Component component);
    Task<Component?> FindComponentByName(string name);


    Task<Product.Entities.Product> CreateProduct(Product.Entities.Product product);
    Task<Product.Entities.Product?> FindProductByName(string name);
    Task<Product.Entities.Product?> FindProductByNameAndBrand(string name, string brand);

    Task<ShopProduct?> GetShopProductByShopAndApiUrl(int shopId, string apiUrl);
    Task<ShopProduct?> GetShopProductByShopAndItemId(int shopId, string itemId);

    Task<bool> UpdateShopProduct(ShopProduct shopProduct);

    Task<ShopProduct> CreateShopProduct(ShopProduct shopProduct);

    Task<List<IShopProductCategory>> GetShopProductCategories(long shopProductId);

    Task<IShopProductCategory> AddShopProductCategory(IShopProductCategory shopCategory);

    Task<IShopProductCategory> AddShopProductCategory(long shopProductId, int shopCategoryId);

    Task<bool> CheckShopProductCategory(long shopProductId, int shopCategoryId);

    Task<ProductComponent> SetProductComponent(ProductComponent productComponent);
    Task<Currency> CreateCurrency(Currency currency);
    Task<Currency?> GetCurrencyByName(string name);
    Task<Currency?> GetCurrencyByCode(short code);

    Task<bool> UpdateShopProductPrice(ShopProductPrice shopProductPrice);

    Task<ShopProductPrice> CreateShopProductPrice(ShopProductPrice shopProductPrice);
    Task<ShopProductPrice?> GetShopProductPrice(long shopProductId);
}
