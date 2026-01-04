using Alchemist.Product.Interfaces;

namespace Alchemist.DataService.Interfaces;

public interface IAlchemyRepository
{
    Task<IProductType> CreateProductType(IProductType productType, CancellationToken cancellationToken = default);
    Task<IProductType?> FindProductTypeByName(string name, CancellationToken cancellationToken = default);

    Task<IPurposeType> CreatePurposeType(IPurposeType purposeType, CancellationToken cancellationToken = default);
    Task<IPurposeType?> FindPurposeTypeByName(string name, CancellationToken cancellationToken = default);

    Task<ICountry> CreateCountry(ICountry country, CancellationToken cancellationToken = default);
    Task<ICountry?> FindCountryByName(string name, CancellationToken cancellationToken = default);

    Task<IBrand> CreateBrand(IBrand brand, CancellationToken cancellationToken = default);
    Task<IBrand?> FindBrandByName(string name, CancellationToken cancellationToken = default);

    Task<IComponent> CreateComponent(IComponent component, CancellationToken cancellationToken = default);
    Task<IComponent?> GetComponent(int id, CancellationToken cancellationToken = default);
    Task<IComponent?> FindComponentByName(string name, CancellationToken cancellationToken = default);

    Task<IShop> CreateShop(IShop shop, CancellationToken cancellationToken = default);

    Task<IShop> UpdateShop(IShop shop, CancellationToken cancellationToken = default);

    Task<IShop?> GetShop(int id, CancellationToken cancellationToken = default);
    Task<List<IShop>> GetShops(CancellationToken cancellationToken = default);
    Task<IShop?> GetShopByName(string name, CancellationToken cancellationToken = default);

    Task<IShop?> GetShopByUrl(string url, CancellationToken cancellationToken = default);
    Task<IProduct> CreateProduct(IProduct product, CancellationToken cancellationToken = default);
    Task<IProduct?> GetProduct(long id, CancellationToken cancellationToken = default);    
    Task<IProduct?> FindProductByName(string name, CancellationToken cancellationToken = default);

    Task<IProduct?> FindProductByNameAndBrand(string name, string brand, CancellationToken cancellationToken = default);

    Task<List<IPurposeType>> GetProductPurposes(long productId, CancellationToken cancellationToken = default);

    Task<IProductPurpose> SetProductPurpose(IProductPurpose productPurpose, CancellationToken cancellationToken = default);

    Task<List<IShopCategory>> GetShopCategories(int shopId, CancellationToken cancellationToken = default);

    Task<IShopCategory?> GetShopCategory(int shopId, int itemId, CancellationToken cancellationToken = default);

    Task<List<IShopCategory>> GetAllCategoryChildren(int parentId, CancellationToken cancellationToken = default);

    Task<IShopCategory> AddShopCategory(IShopCategory shopCategory, CancellationToken cancellationToken = default);

    Task<IShopProduct> CreateShopProduct(IShopProduct shopProduct, CancellationToken cancellationToken = default);

    Task<IShopProduct?> GetShopProductByShopAndApiUrl(int shopId, string itemUrl, CancellationToken cancellationToken = default);

    Task<IShopProduct?> GetShopProductByShopAndItemId(int shopId, string itemId, CancellationToken cancellationToken = default);

    Task<IShopProduct?> GetShopProductByShopAndProductId(int shopId, long productId, CancellationToken cancellationToken = default);

    Task<bool> UpdateShopProduct(IShopProduct shopProduct, CancellationToken cancellationToken = default);

    Task<List<IShopProductCategory>> GetShopProductCategories(long shopProductId, CancellationToken cancellationToken = default);

    Task<IShopProductCategory> AddShopProductCategory(IShopProductCategory shopCategory, CancellationToken cancellationToken = default);

    Task<IShopProductCategory> AddShopProductCategory(long shopProductId, int shopCategoryId, CancellationToken cancellationToken = default);

    Task<bool> CheckShopProductCategory(long shopProductId, int shopCategoryId, CancellationToken cancellationToken = default);

    Task<IProductComponent> SetProductComponent(IProductComponent productComponent, CancellationToken cancellationToken = default);

    Task<ICurrency?> GetCurrencyByName(string name, CancellationToken cancellationToken = default);

    Task<ICurrency?> GetCurrencyByCode(short code, CancellationToken cancellationToken = default);

    Task<ICurrency> CreateCurrency(ICurrency currency, CancellationToken cancellationToken = default);

    Task<IShopProductPrice?> GetShopProductPrice(long shopProductId, CancellationToken cancellationToken = default);

    Task<bool> UpdateShopProductPrice(IShopProductPrice shopProductPrice, CancellationToken cancellationToken = default);

    Task<IShopProductPrice> CreateShopProductPrice(IShopProductPrice shopProductPrice, CancellationToken cancellationToken = default);
}