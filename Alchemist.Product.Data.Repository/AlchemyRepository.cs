using Alchemist.DataService.Interfaces;
using Alchemist.Exceptions;
using Alchemist.Product.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Alchemist.Product.Data.Repository;

public class AlchemyRepository(AlchemyContext context) : IAlchemyRepository, IAsyncDisposable
{
    protected AlchemyContext Context { get; set; } = context;

    public async Task<IBrand> CreateBrand(IBrand brand)
    {
        var brandEntity = brand.To<Brand>();
        return await Context.Create(brandEntity);
    }

    public async Task<IComponent> CreateComponent(IComponent component)
    {
        var componentEntity = component.To<Component>();
        return await Context.Create(componentEntity);
    }

    public async Task<IComponentGroup> CreateComponentGroup(IComponentGroup componentGroup)
    {
        var componentGroupEntity = componentGroup.To<ComponentGroup>();
        return await Context.Create(componentGroupEntity);
    }

    public async Task<ICountry> CreateCountry(ICountry country)
    {
        var countryEntity = country.To<Country>();
        return await Context.Create(countryEntity);
    }

    public async Task<IProduct> CreateProduct(IProduct product)
    {
        var productEntity = product.To<Product>();
        return await Context.Create(productEntity);
    }

    public async Task<IPurposeType> CreatePurposeType(IPurposeType purposeType)
    {
        var purposeTypeEntity = purposeType.To<PurposeType>();
        return await Context.Create(purposeTypeEntity);
    }

    public async Task<IShop> CreateShop(IShop shop)
    {
        var shopEntity = shop.To<Shop>();
        return await Context.Create(shopEntity);
    }

    public List<IBrand> GetBrands()
    {
        return [.. Context.Brands.OfType<IBrand>()];
    }

    public async Task<IComponent?> GetComponent(int id)
    {
        return await Context.GetById<Component, int>(id);
    }

    public List<IComponentGroup> GetComponentGroups()
    {
        return [.. Context.ComponentGroups.OfType<IComponentGroup>()];
    }

    public List<IComponentGroup> GetComponentGroupsByParent(int parentGroupId)
    {
        throw new NotImplementedException();
    }

    public List<IComponentGroup> GetComponentGroupsByPurposeType(int purposeTypeId)
    {
        throw new NotImplementedException();
    }

    public List<IComponent> GetComponents()
    {
        return [.. Context.Components.OfType<IComponent>()];
    }

    public List<IComponent> GetComponentsByGroup(int groupId)
    {
        throw new NotImplementedException();
    }

    public List<IComponent> GetComponentsByPurposeType(short purposeTypeId)
    {
        throw new NotImplementedException();
    }

    public List<ICountry> GetCountries()
    {
        return [.. Context.Countries.OfType<ICountry>()];
    }

    public async Task<IProduct?> GetProduct(long id)
    {
        return await Context.GetById<Product, long>(id);
    }

    public List<IProduct> GetProductsByShop(int shopId)
    {
        throw new NotImplementedException();
    }

    public List<IProductType> GetProductTypes()
    {
        return [.. Context.ProductTypes.OfType<IProductType>()];
    }

    public List<IPurposeType> GetPurposeTypes()
    {
        return [.. Context.PurposeTypes.OfType<IPurposeType>()];
    }

    public async Task<IShop?> GetShop(int id)
    {
        return await Context.GetById<Shop, int>(id);
    }

    public async Task<List<IShop>> GetShops()
    {
        return await Context.Shops.OfType<IShop>().ToListAsync();
    }

    public async Task<IProductType> CreateProductType(IProductType productType)
    {
        var productTypeEntity = productType.To<ProductType>();
        return await Context.Create(productTypeEntity);
    }

    public async Task<List<IShopCategory>> GetShopCategories(int shopId)
    {
        var shopCategories = await Context.ShopCategories.Where(su => su.ShopId == shopId).OfType<IShopCategory>().ToListAsync();
        return shopCategories;
    }

    public async Task<IShopCategory> AddShopCategory(IShopCategory shopCategory)
    {
        var shopCategoryEntity = shopCategory.To<ShopCategory>();
        return await Context.Create(shopCategoryEntity);
    }

    public async Task<IShopProductCategory> AddShopProductCategory(long shopProductId, int shopCategoryId)
    {
        var shopProductCategoryEntity = new ShopProductCategory { ShopProductId = shopProductId, ShopCategoryId = shopCategoryId };
        return await Context.Create(shopProductCategoryEntity);
    }

    public async Task<bool> CheckShopProductCategory(long shopProductId, int shopCategoryId)
    {
        return await Context.ShopProductCategories.AnyAsync(s => s.ShopProductId == shopProductId && s.ShopCategoryId == s.ShopCategoryId);
    }

    public async Task<List<IShopProductCategory>> GetShopProductCategories(long shopProductId)
    {
        return await Context.ShopProductCategories.Where(s => s.ShopProductId == shopProductId).OfType<IShopProductCategory>().ToListAsync();
    }

    public async Task<IShopProductCategory> AddShopProductCategory(IShopProductCategory shopProductCategory)
    {
        var shopProductCategoryEntity = shopProductCategory.To<ShopProductCategory>();
        return await Context.Create(shopProductCategoryEntity);
    }

    public async Task<IShop?> GetShopByName(string name)
    {
        var shops = await Context.Shops.Where(s => s.Name.ToLower() == name.ToLower()).ToListAsync();
        if (shops.Count > 1)
            throw new Exception(string.Format($"multiple shops with name {name}"));
        return await Task.FromResult(shops.FirstOrDefault());
    }

    public async Task<IShop?> GetShopByUrl(string url)
    {
        var shops = await Context.Shops.Where(s => s.Url.ToLower() == url.ToLower()).ToListAsync();
        if (shops.Count > 1)
            throw new Exception(string.Format($"multiple shops with url {0}"));
        return await Task.FromResult(shops.FirstOrDefault());
    }

    public async Task<IShopProduct> CreateShopProduct(IShopProduct shopProduct)
    {
        var shopProductEntity = shopProduct.To<ShopProduct>();
        return await Context.Create(shopProductEntity);
    }

    public async Task<IProductType?> FindProductTypeByName(string name)
    {
        return await Context.FindByName<ProductType, short>(name);
    }

    public async Task<IPurposeType?> FindPurposeTypeByName(string name)
    {
        return await Context.FindByName<PurposeType, short>(name);
    }

    public async Task<ICountry?> FindCountryByName(string name)
    {
        return await Context.FindByName<Country, short>(name);
    }

    public async Task<IBrand?> FindBrandByName(string name)
    {
        return await Context.FindByName<Brand, int>(name);
    }

    public async Task<IComponent?> FindComponentByName(string name)
    {
        return await Context.FindByName<Component, int>(name);
    }

    public async Task<IProduct?> FindProductByName(string name)
    {
        return await Context.FindByName<Product, long>(name, (p) => [p.Transcript ?? ""]);
    }

    public async Task<IProduct?> FindProductByNameAndBrand(string name, string brand)
    {
        var brands = await Context.Brands.Where(b => b.Name.Trim().ToUpper() == brand.Trim().ToUpper()).ToListAsync();
        if (brands.Count == 0) return default;
        var products = await Context.Products.Where(p => p.Name.Trim().ToUpper() == name.Trim().ToUpper()).ToListAsync();
        products = [.. products.Where(p => brands.Any(b => b.Id == p.Id))];
        return products.Count > 1
            ? throw new WarningException($"multiple products with name {name} and brand {brand}", products.FirstOrDefault())
            : (IProduct?)await Task.FromResult(products.FirstOrDefault());
    }

    public async Task<IShopProduct?> GetShopProductByShopAndApiUrl(int shopId, string apiUrl)
    {
        return await Context.ShopProducts.Where(sp => sp.ShopId == shopId && sp.ApiUrl.Trim() == apiUrl.Trim()).FirstOrDefaultAsync();
    }

    public async Task<IShopProduct?> GetShopProductByShopAndItemId(int shopId, string itemId)
    {
        return await Context.ShopProducts.Where(sp => sp.ShopId == shopId && sp.ItemId.Trim() == itemId.Trim()).FirstOrDefaultAsync();
    }

    public async Task<IShopProduct?> GetShopProductByShopAndProductId(int shopId, long productId)
    {
        return await Context.ShopProducts.Where(sp => sp.ShopId == shopId && sp.ProductId == productId).FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateShopProduct(IShopProduct shopProduct)
    {
        var shopProductEntity = shopProduct.To<ShopProduct>();
        var updated = Context.ShopProducts.Update(shopProductEntity);
        var savedCount = await Context.SaveChangesAsync();
        return await Task.FromResult(savedCount >= 1);
    }

    public async Task<IProductComponent> SetProductComponent(IProductComponent productComponent)
    {
        var existed = await Context.ProductComponents.FindAsync(productComponent.ProductId, productComponent.ComponentId);

        if (existed == null)
        {
            var entity = productComponent.To<ProductComponent>();
            return await Context.Create(entity);
        }
        else if (existed.SequalNumber != productComponent.SequalNumber)
        {
            existed.SequalNumber = productComponent.SequalNumber;
            var updated = Context.Update(existed);
            var savedCount = await Context.SaveChangesAsync();
            return await Task.FromResult(existed);
        }

        return await Task.FromResult(existed);
    }

    public async Task<ICurrency?> GetCurrencyByName(string name)
    {
        return await Context.FindByName<Currency, short>(name);
    }

    public async Task<ICurrency> CreateCurrency(ICurrency currency)
    {
        var currencyEntity = currency.To<Currency>();
        return await Context.Create(currencyEntity);
    }

    public async Task<ICurrency?> GetCurrencyByCode(short code)
    {
        var entities = Context.Currencies.Where(e => e.Code == code).ToList();
        return entities.Count > 1
            ? throw new WarningException($"multiple currencies with code {code}", entities.FirstOrDefault())
            : (ICurrency?)await Task.FromResult(entities.FirstOrDefault());
    }

    public async Task<bool> UpdateShopProductPrice(IShopProductPrice shopProductPrice)
    {
        var entity = shopProductPrice.To<ShopProductPrice>();
        var updated = Context.ShopProductPrices.Update(entity);
        var savedCount = await Context.SaveChangesAsync();
        return await Task.FromResult(savedCount >= 1);
    }

    public async Task<IShopProductPrice> CreateShopProductPrice(IShopProductPrice shopProductPrice)
    {
        var entity = shopProductPrice.To<ShopProductPrice>();
        return await Context.Create(entity);
    }

    public async Task<IShopProductPrice?> GetShopProductPrice(long shopProductId)
    {
        return await Context.ShopProductPrices.FirstOrDefaultAsync(spp => spp.ShopProductId == shopProductId);
    }

    public async Task<IShopCategory?> GetShopCategory(int shopId, int itemId)
    {
        var shopCategory = await Context.ShopCategories.FirstOrDefaultAsync(sc => sc.ShopId == shopId && sc.ItemId == itemId);
        return shopCategory;
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
    }

    public async Task<IShop> UpdateShop(IShop shop)
    {
        var shopEntity = shop.To<Shop>();
        var updated = Context.Shops.Update(shopEntity);
        var savedCount = await Context.SaveChangesAsync();
        return updated.Entity;
    }

    private readonly SemaphoreSlim _addCategoryChildrenSemaphore = new(1, 1);
    public async Task<List<IShopCategory>> GetAllCategoryChildren(int parentId)
    {
        var children = await Context.ShopCategories
            .Where(c => c.ParentId == parentId).ToListAsync();

        var tokenSource = new CancellationTokenSource();

        var result = new List<IShopCategory>();

        foreach (var child in children)
        {
            var categoryChildren = await GetCategoryChildrenTree(child.Id, tokenSource.Token);
            await AddCategoryChildren(result, categoryChildren, tokenSource.Token);
        }

        return [.. children.Union(result)];
    }

    private async Task AddCategoryChildren(List<IShopCategory> categories, IEnumerable<IShopCategory> children, CancellationToken token)
    {
        await _addCategoryChildrenSemaphore.WaitAsync(token);
        categories.AddRange(children);
        _addCategoryChildrenSemaphore.Release();
    }

    private async Task<List<IShopCategory>> GetCategoryChildrenTree(int parentId, CancellationToken token)
    {
        var children = await Context.ShopCategories.Where(c => c.ParentId == parentId).ToListAsync(cancellationToken: token);

        var next = new List<IShopCategory>();

        foreach (var child in children)
        {
            var childrenTree = await GetCategoryChildrenTree(child.Id, token);
            next.AddRange(childrenTree);
        }

        return [.. next.Union(children)];
    }

    public async Task<List<IPurposeType>> GetProductPurposes(long productId)
    {
        var purposeTypes = await Context.ProductPurposes.Where(pp => pp.ProductId == productId).Join(Context.PurposeTypes, pp => pp.PurposeTypeId, pt => pt.Id,
                 (pp, pt) => new PurposeType { Id = pt.Id, Name = pt.Name }).ToListAsync();

        return await Task.FromResult(purposeTypes.OfType<IPurposeType>().ToList());
    }

    public async Task<IProductPurpose> SetProductPurpose(IProductPurpose productPurpose)
    {
        var existed = await Context.ProductPurposes.FirstOrDefaultAsync(
            pp => pp.ProductId == productPurpose.ProductId && pp.PurposeTypeId == productPurpose.PurposeTypeId);

        if (existed == null)
        {
            var entity = productPurpose.To<ProductPurpose>();
            return await Context.Create(entity);
        }
        else
        {
            var updated = Context.Update(existed);
            return await Task.FromResult(updated.Entity);
        }
    }
}
