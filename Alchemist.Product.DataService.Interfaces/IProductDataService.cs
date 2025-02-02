using Alchemist.Product.Entities;

namespace Alchemist.Product.DataService.Interfaces;

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

    
    Task<Entities.Product> CreateProduct(Entities.Product product);
    Task<Entities.Product?> FindProductByName(string name);
    Task<Entities.Product?> FindProductByNameAndBrand(string name, string brand);

    Task<ShopProduct?> GetShopProductByShopAndApiUrl(int shopId, string apiUrl);
    Task<ShopProduct?> GetShopProductByShopAndItemId(int shopId, string itemId);

    Task<bool> UpdateShopProduct(ShopProduct shopProduct);

    Task<ShopProduct> CreateShopProduct(ShopProduct shopProduct);

    Task<ProductComponent> SetProductComponent(ProductComponent productComponent);
    Task<Currency> CreateCurrency(Currency currency);
    Task<Currency?> GetCurrencyByName(string name);
    Task<Currency?> GetCurrencyByCode(short code);

    Task<bool> UpdateShopProductPrice(ShopProductPrice shopProductPrice);

    Task<ShopProductPrice> CreateShopProductPrice(ShopProductPrice shopProductPrice);
    Task<ShopProductPrice?> GetShopProductPrice(long shopProductId);
}
