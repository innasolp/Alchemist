using Alchemist.Product.Interfaces;

namespace Alchemist.Product.DataService.Interfaces;

public interface IAlchemyRepository
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
    Task<IComponent?> GetComponent(int id);
    Task<IComponent?> FindComponentByName(string name);

    Task<IShop> CreateShop(IShop shop);
    Task<IShop?> GetShop(int id);
    Task<IShop?> GetShopByName(string name);

    Task<IShop?> GetShopByUrl(string url);

    //List<IProduct> GetProducts();
    Task<IProduct> CreateProduct(IProduct product);
    Task<IProduct?> GetProduct(long id);
    List<IProduct> GetProductsByShop(int shopId);
    Task<IProduct?> FindProductByName(string name);

    Task<IShopUrl?> GetShopUrl(int shopId);

    Task<List<IShopCategory>> GetShopCategories(int shopId);
    Task<IShopCategory?> GetShopCategory(int shopId, int itemId);

    Task<IShopUrl> AddShopUrl(IShopUrl shopUrl);

    Task<IShopCategory> AddShopCategory(IShopCategory shopCategory);

    Task<IShopProduct> CreateShopProduct(IShopProduct shopProduct);
    Task<IShopProduct?> GetShopProductByShopAndApiUrl(int shopId, string itemUrl);

    Task<IShopProduct?> GetShopProductByShopAndItemId(int shopId, string itemId);

    Task<bool> UpdateShopProduct(IShopProduct shopProduct);

    Task<List<IShopProductCategory>> GetShopProductCategories(long shopProductId);

    Task<IShopProductCategory> AddShopProductCategory(IShopProductCategory shopCategory);

    Task<IShopProductCategory> AddShopProductCategory(long shopProductId, int shopCategoryId);

    Task<IProductComponent> SetProductComponent(IProductComponent productComponent);

    Task<ICurrency?> GetCurrencyByName(string name);

    Task<ICurrency?> GetCurrencyByCode(short code);

    Task<ICurrency> CreateCurrency(ICurrency currency);

    Task<IShopProductPrice?> GetShopProductPrice(long shopProductId);

    Task<bool> UpdateShopProductPrice(IShopProductPrice shopProductPrice);

    Task<IShopProductPrice> CreateShopProductPrice(IShopProductPrice shopProductPrice);
}
