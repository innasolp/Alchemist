using Alchemist.DataService.Interfaces;
using Alchemist.Exceptions;
using Alchemist.Product.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Alchemist.Product.Data.Repository;

public class AlchemyRepository(AlchemyContext context) : IAlchemyRepository, IAsyncDisposable
{
    protected AlchemyContext Context { get; } = context;

    public async Task<IBrand> CreateBrand(IBrand brand, CancellationToken cancellationToken = default)
    {
        var brandEntity = brand.To<Brand>();
        return await Context.Create(brandEntity, cancellationToken);
    }

    public async Task<IComponent> CreateComponent(IComponent component, CancellationToken cancellationToken = default)
    {
        var componentEntity = component.To<Component>();
        return await Context.Create(componentEntity, cancellationToken);
    }

    public async Task<IComponentGroup> CreateComponentGroup(IComponentGroup componentGroup, CancellationToken cancellationToken = default)
    {
        var componentGroupEntity = componentGroup.To<ComponentGroup>();
        return await Context.Create(componentGroupEntity, cancellationToken);
    }

    public async Task<ICountry> CreateCountry(ICountry country, CancellationToken cancellationToken = default)
    {
        var countryEntity = country.To<Country>();
        return await Context.Create(countryEntity, cancellationToken);
    }

    public async Task<IProduct> CreateProduct(IProduct product, CancellationToken cancellationToken = default)
    {
        var productEntity = product.To<Product>();
        return await Context.Create(productEntity, cancellationToken);
    }

    public async Task<IPurposeType> CreatePurposeType(IPurposeType purposeType, CancellationToken cancellationToken = default)
    {
        var purposeTypeEntity = purposeType.To<PurposeType>();
        return await Context.Create(purposeTypeEntity, cancellationToken);
    }

    public async Task<IShop> CreateShop(IShop shop, CancellationToken cancellationToken = default)
    {
        var shopEntity = shop.To<Shop>();
        return await Context.Create(shopEntity, cancellationToken);
    }

    public List<IBrand> GetBrands() => [.. Context.Brands.ToList().OfType<IBrand>()];

    public async Task<IComponent?> GetComponent(int id, CancellationToken cancellationToken = default)
    {
        return await Context.GetById<Component, int>(id, cancellationToken);
    }    

    public async Task<IProduct?> GetProduct(long id, CancellationToken cancellationToken = default)
    {
        return await Context.GetById<Product, long>(id, cancellationToken);
    }    

    public async Task<IShop?> GetShop(int id, CancellationToken cancellationToken = default)
    {
        return await Context.GetById<Shop, int>(id, cancellationToken);
    }

    public async Task<List<IShop>> GetShops(CancellationToken cancellationToken = default)
    {
        return await Context.Shops.ToListAsync(cancellationToken).ContinueWith(t => t.Result.OfType<IShop>().ToList(), cancellationToken);
    }

    public async Task<IProductType> CreateProductType(IProductType productType, CancellationToken cancellationToken = default)
    {
        var productTypeEntity = productType.To<ProductType>();
        return await Context.Create(productTypeEntity, cancellationToken);
    }

    public async Task<List<IShopCategory>> GetShopCategories(int shopId, CancellationToken cancellationToken = default)
    {
        var shopCategories = await Context.ShopCategories.Where(su => su.ShopId == shopId).ToListAsync(cancellationToken);
        return [.. shopCategories.OfType<IShopCategory>()];
    }

    public async Task<IShopCategory> AddShopCategory(IShopCategory shopCategory, CancellationToken cancellationToken = default)
    {
        var shopCategoryEntity = shopCategory.To<ShopCategory>();
        return await Context.Create(shopCategoryEntity, cancellationToken);
    }

    public async Task<IShopProductCategory> AddShopProductCategory(long shopProductId, int shopCategoryId, CancellationToken cancellationToken = default)
    {
        var shopProductCategoryEntity = new ShopProductCategory { ShopProductId = shopProductId, ShopCategoryId = shopCategoryId };
        return await Context.Create(shopProductCategoryEntity, cancellationToken);
    }

    public async Task<bool> CheckShopProductCategory(long shopProductId, int shopCategoryId, CancellationToken cancellationToken = default)
    {
        return await Context.ShopProductCategories.AnyAsync(s => s.ShopProductId == shopProductId && s.ShopCategoryId == shopCategoryId, cancellationToken);
    }

    public async Task<List<IShopProductCategory>> GetShopProductCategories(long shopProductId, CancellationToken cancellationToken = default)
    {
        var list = await Context.ShopProductCategories.Where(s => s.ShopProductId == shopProductId).ToListAsync(cancellationToken);
        return [.. list.OfType<IShopProductCategory>()];
    }

    public async Task<IShopProductCategory> AddShopProductCategory(IShopProductCategory shopProductCategory, CancellationToken cancellationToken = default)
    {
        var shopProductCategoryEntity = shopProductCategory.To<ShopProductCategory>();
        return await Context.Create(shopProductCategoryEntity, cancellationToken);
    }

    public async Task<IShop?> GetShopByName(string name, CancellationToken cancellationToken = default)
    {
        //StringComparison not available in EF Core queries
        var shops = await Context.Shops.Where(s => s.Name.ToUpper() == name.ToUpper()).ToListAsync(cancellationToken);
        if (shops.Count > 1)
            throw new Exception($"multiple shops with name {name}");
        return shops.FirstOrDefault();
    }

    public async Task<IShop?> GetShopByUrl(string url, CancellationToken cancellationToken = default)
    {
        //StringComparison not available in EF Core queries
        var shops = await Context.Shops.Where(s => s.Url.ToUpper() ==url.ToUpper()).ToListAsync(cancellationToken);
        if (shops.Count > 1)
            throw new Exception($"multiple shops with url {url}");
        return shops.FirstOrDefault();
    }

    public async Task<IShopProduct> CreateShopProduct(IShopProduct shopProduct, CancellationToken cancellationToken = default)
    {
        var shopProductEntity = shopProduct.To<ShopProduct>();
        return await Context.Create(shopProductEntity, cancellationToken);
    }

    public async Task<IProductType?> FindProductTypeByName(string name, CancellationToken cancellationToken = default)
    {
        return await Context.FindByName<ProductType, short>(name, cancellationToken);
    }

    public async Task<IPurposeType?> FindPurposeTypeByName(string name, CancellationToken cancellationToken = default)
    {
        return await Context.FindByName<PurposeType, short>(name, cancellationToken);
    }

    public async Task<ICountry?> FindCountryByName(string name, CancellationToken cancellationToken = default)
    {
        return await Context.FindByName<Country, short>(name, cancellationToken);
    }

    public async Task<IBrand?> FindBrandByName(string name, CancellationToken cancellationToken = default)
    {
        return await Context.FindByName<Brand, int>(name, cancellationToken);
    }

    public async Task<IComponent?> FindComponentByName(string name, CancellationToken cancellationToken = default)
    {
        return await Context.FindByName<Component, int>(name, cancellationToken);
    }

    public async Task<IProduct?> FindProductByName(string name, CancellationToken cancellationToken = default)
    {
        return await Context.FindByName<Product, long>(name, p => [p.Transcript ?? string.Empty], cancellationToken);
    }

    public async Task<IProduct?> FindProductByNameAndBrand(string name, string brand, CancellationToken cancellationToken = default)
    {
        //StringComparison not available in EF Core queries
        var brands = await Context.Brands.Where(b => b.Name.Trim().ToUpper() == brand.Trim().ToUpper()).ToListAsync(cancellationToken);
        if (brands.Count == 0) return default;
        var products = await Context.Products.Where(p => p.Name.Trim().ToUpper() == name.Trim().ToUpper()).ToListAsync(cancellationToken);
        products = [.. products.Where(p => brands.Any(b => b.Id == p.BrandId))];
        return products.Count > 1
            ? throw new WarningException($"multiple products with name {name} and brand {brand}", products.FirstOrDefault())
            : (IProduct?)products.FirstOrDefault();
    }

    public async Task<IShopProduct?> GetShopProductByShopAndApiUrl(int shopId, string apiUrl, CancellationToken cancellationToken = default)
    {
        return await Context.ShopProducts.Where(sp => sp.ShopId == shopId && sp.ApiUrl.Trim() == apiUrl.Trim()).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IShopProduct?> GetShopProductByShopAndItemId(int shopId, string itemId, CancellationToken cancellationToken = default)
    {
        return await Context.ShopProducts.Where(sp => sp.ShopId == shopId && sp.ItemId.Trim() == itemId.Trim()).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IShopProduct?> GetShopProductByShopAndProductId(int shopId, long productId, CancellationToken cancellationToken = default)
    {
        return await Context.ShopProducts.Where(sp => sp.ShopId == shopId && sp.ProductId == productId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> UpdateShopProduct(IShopProduct shopProduct, CancellationToken cancellationToken = default)
    {
        var shopProductEntity = shopProduct.To<ShopProduct>();
        var updated = Context.ShopProducts.Update(shopProductEntity);
        var savedCount = await Context.SaveChangesAsync(cancellationToken);
        return savedCount >= 1;
    }

    public async Task<IProductComponent> SetProductComponent(IProductComponent productComponent, CancellationToken cancellationToken = default)
    {
        var existed = await Context.ProductComponents.FindAsync([productComponent.ProductId, productComponent.ComponentId], cancellationToken);

        if (existed == null)
        {
            var entity = productComponent.To<ProductComponent>();
            return await Context.Create(entity, cancellationToken);
        }
        else if (existed.SequalNumber != productComponent.SequalNumber)
        {
            existed.SequalNumber = productComponent.SequalNumber;
            Context.Update(existed);
            await Context.SaveChangesAsync(cancellationToken);
            return existed;
        }

        return existed;
    }

    public async Task<ICurrency?> GetCurrencyByName(string name, CancellationToken cancellationToken = default)
    {
        return await Context.FindByName<Currency, short>(name, cancellationToken);
    }

    public async Task<ICurrency> CreateCurrency(ICurrency currency, CancellationToken cancellationToken = default)
    {
        var currencyEntity = currency.To<Currency>();
        return await Context.Create(currencyEntity, cancellationToken);
    }

    public async Task<ICurrency?> GetCurrencyByCode(short code, CancellationToken cancellationToken = default)
    {
        var entities = await Context.Currencies.Where(e => e.Code == code).ToListAsync(cancellationToken);
        return entities.Count > 1
            ? throw new WarningException($"multiple currencies with code {code}", entities.FirstOrDefault())
            : (ICurrency?)entities.FirstOrDefault();
    }

    public async Task<bool> UpdateShopProductPrice(IShopProductPrice shopProductPrice, CancellationToken cancellationToken = default)
    {
        var entity = shopProductPrice.To<ShopProductPrice>();
        Context.ShopProductPrices.Update(entity);
        var savedCount = await Context.SaveChangesAsync(cancellationToken);
        return savedCount >= 1;
    }

    public async Task<IShopProductPrice> CreateShopProductPrice(IShopProductPrice shopProductPrice, CancellationToken cancellationToken = default)
    {
        var entity = shopProductPrice.To<ShopProductPrice>();
        return await Context.Create(entity, cancellationToken);
    }

    public async Task<IShopProductPrice?> GetShopProductPrice(long shopProductId, CancellationToken cancellationToken = default)
    {
        return await Context.ShopProductPrices.FirstOrDefaultAsync(spp => spp.ShopProductId == shopProductId, cancellationToken);
    }

    public async Task<IShopCategory?> GetShopCategory(int shopId, int itemId, CancellationToken cancellationToken = default)
    {
        return await Context.ShopCategories.FirstOrDefaultAsync(sc => sc.ShopId == shopId && sc.ItemId == itemId, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    public async Task<IShop> UpdateShop(IShop shop, CancellationToken cancellationToken = default)
    {
        var shopEntity = shop.To<Shop>();
        var updated = Context.Shops.Update(shopEntity);
        await Context.SaveChangesAsync(cancellationToken);
        return updated.Entity;
    }

    private readonly SemaphoreSlim _addCategoryChildrenSemaphore = new(1, 1);

    public async Task<List<IShopCategory>> GetAllCategoryChildren(int parentId, CancellationToken cancellationToken = default)
    {
        var children = await Context.ShopCategories
            .Where(c => c.ParentId == parentId).ToListAsync(cancellationToken);

        var result = new List<IShopCategory>();

        foreach (var child in children)
        {
            var categoryChildren = await GetCategoryChildrenTree(child.Id, cancellationToken);
            await AddCategoryChildren(result, categoryChildren, cancellationToken);
        }

        return [.. children.Union(result)];
    }

    private async Task AddCategoryChildren(List<IShopCategory> categories, IEnumerable<IShopCategory> children, CancellationToken token)
    {
        await _addCategoryChildrenSemaphore.WaitAsync(token);
        try
        {
            categories.AddRange(children);
        }
        finally
        {
            _addCategoryChildrenSemaphore.Release();
        }
    }

    private async Task<List<IShopCategory>> GetCategoryChildrenTree(int parentId, CancellationToken token)
    {
        var children = await Context.ShopCategories.Where(c => c.ParentId == parentId).ToListAsync(token);

        var next = new List<IShopCategory>();

        foreach (var child in children)
        {
            var childrenTree = await GetCategoryChildrenTree(child.Id, token);
            next.AddRange(childrenTree);
        }

        return [.. next.Union(children)];
    }

    public async Task<List<IPurposeType>> GetProductPurposes(long productId, CancellationToken cancellationToken = default)
    {
        var purposeTypes = await Context.ProductPurposes
            .Where(pp => pp.ProductId == productId)
            .Join(Context.PurposeTypes, pp => pp.PurposeTypeId, pt => pt.Id,
                  (pp, pt) => new PurposeType { Id = pt.Id, Name = pt.Name })
            .ToListAsync(cancellationToken);

        return [.. purposeTypes.OfType<IPurposeType>()];
    }

    public async Task<IProductPurpose> SetProductPurpose(IProductPurpose productPurpose, CancellationToken cancellationToken = default)
    {
        var existed = await Context.ProductPurposes.FirstOrDefaultAsync(
            pp => pp.ProductId == productPurpose.ProductId && pp.PurposeTypeId == productPurpose.PurposeTypeId,
            cancellationToken);

        if (existed == null)
        {
            var entity = productPurpose.To<ProductPurpose>();
            return await Context.Create(entity, cancellationToken);
        }
        else
        {
            var updated = Context.Update(existed);
            await Context.SaveChangesAsync(cancellationToken);
            return updated.Entity;
        }
    }
}