using Alchemist.Product.Interfaces;

namespace Alchemist.Product.DataService.Interfaces;

public interface IProductDataService
{
    Task<IProductType> CreateProductType(IProductType productType);
    Task<IProductType?> FindProductTypeByName(string name);

    Task<IPurposeType> CreatePurposeType(IPurposeType purposeType);
    Task<IPurposeType?> FindPurposeTypeByName(string name);

    Task<ICountry> CreateCountry(ICountry country);
    Task<ICountry?> FindCountryByName(string name);

    Task<IBrand> CreateBrand(IBrand brand);
    Task<IBrand?> FindBrandByName(string name);

    Task<IComponent> CreateComponent(IComponent component);
    Task<IComponent?> FindComponentByName(string name);


    Task<IProduct> CreateProduct(IProduct product);
    Task<IProduct?> FindProductByName(string name);
    Task<IProduct?> FindProductByNameAndBrand(string name, string brand);

    Task<IShopProduct?> GetShopProductByShopAndApiUrl(int shopId, string apiUrl);
    Task<IShopProduct?> GetShopProductByShopAndItemId(int shopId, string itemId);
    Task<IShopProduct?> GetShopProductByShopAndProductId(int shopId, long productId);

    Task<bool> UpdateShopProduct(IShopProduct shopProduct);

    Task<IShopProduct> CreateShopProduct(IShopProduct shopProduct);

    Task<List<IShopProductCategory>> GetShopProductCategories(long shopProductId);

    Task<IShopProductCategory> AddShopProductCategory(IShopProductCategory shopCategory);

    Task<IShopProductCategory> AddShopProductCategory(long shopProductId, int shopCategoryId);

    Task<bool> CheckShopProductCategory(long shopProductId, int shopCategoryId);

    Task<IProductComponent> SetProductComponent(IProductComponent productComponent);
    Task<ICurrency> CreateCurrency(ICurrency currency);
    Task<ICurrency?> GetCurrencyByName(string name);
    Task<ICurrency?> GetCurrencyByCode(short code);

    Task<bool> UpdateShopProductPrice(IShopProductPrice shopProductPrice);

    Task<IShopProductPrice> CreateShopProductPrice(IShopProductPrice shopProductPrice);
    Task<IShopProductPrice?> GetShopProductPrice(long shopProductId);
}
