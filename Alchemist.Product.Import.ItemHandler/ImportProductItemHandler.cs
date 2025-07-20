using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Exceptions;
using Alchemist.Product.Entities;
using Alchemist.Product.ImportItem.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.ImportItem.Handler;

internal class ImportProductItemHandler(IProductDataService productDataService, IShopDataService shopDataService) 
    : IItemHandler<IImportProductItem>
{
    private readonly IProductDataService _productDataService = productDataService;

    private readonly IShopDataService _shopDataService = shopDataService;

    public async Task<ItemProcessStatus> HandleItem(IImportProductItem  productItem)
    {        
        try
        {
            var shopProduct = await _productDataService.GetShopProductByShopAndItemId(productItem.ShopId, productItem.ShopProduct.ItemId)
                ??
                new ShopProduct
                {
                    ShopId = productItem.ShopId,
                    ItemId = productItem.ShopProduct.ItemId,
                    ApiUrl = productItem.ShopProduct.ApiUrl,
                    ItemUrl = productItem.ShopProduct.ItemUrl
                };

            if (shopProduct.ProductId != 0)
            {
                var setCategoryResult = await SetShopProductCategoryIfNeedAsync(productItem.ShopId, shopProduct.Id, productItem.ShopCategory.ItemId);
                //todo
                //if(!categoryResult)
                //    throw new WarningException($"Category {productItem.CategoryId} in shop {shopUrlModel.ShopName} not found. Url {productItem.ApiUrl}");

                var setPriceResult = await SetShopProductPriceForItemAsync(productItem, shopProduct.Id);
                //todo
                //if (!result)
                //    throw new WarningException($"Price for shop product {shopProduct.Id} was not set. Url {productItem.ApiUrl}");            

                var result = !(setPriceResult & setCategoryResult) ? ItemProcessStatus.Error : ItemProcessStatus.Updated;

                return result;
            }

            var product = await _productDataService.FindProductByNameAndBrand(productItem?.Product.Name, productItem?.Brand.Name)
                                ?? await _productDataService.FindProductByName(productItem?.Product.Name);

            if (product != null)
            {
                if (await _productDataService.GetShopProductByShopAndProductId(productItem.ShopId, product.Id) != null)
                    return ItemProcessStatus.AlreadyExists;
            }
            else
                product = await CreateProductFromModelAsync(productItem, productItem.ShopId);

            shopProduct.ProductId = product.Id;
            shopProduct.IsActual = true;

            var newShopProduct = await _productDataService.CreateShopProduct(shopProduct);

            if (!await SetShopProductPriceForItemAsync(productItem, newShopProduct.Id) ||
                    !await SetShopProductCategoryIfNeedAsync(productItem.ShopId, newShopProduct.Id, productItem.ShopCategory.ItemId))
                return await Task.FromResult(ItemProcessStatus.Error);

            //todo    throw new WarningException($"Price for shop product {shopProduct.Id} was not set. Url {productItem.ApiUrl}");
            

            return ItemProcessStatus.New;
        }
        catch (Exception e)
        {           

            throw new WarningException($"Product {productItem.ShopProduct.ItemUrl} proccessed with error.", e);
        }
    }

    private async Task<bool> SetShopProductCategoryIfNeedAsync(int shopId, long shopProductId, int categoryItemId)
    {
        var category = await _shopDataService.GetShopCategoryByShopIdAndItemId(shopId, categoryItemId);
        if (category == null)
            return await Task.FromResult(false);

        var categoryExists = await _productDataService.CheckShopProductCategory(shopProductId, category.Id);
        if (!categoryExists)
            await _productDataService.AddShopProductCategory(shopProductId, category.Id);

        return await Task.FromResult(true);
    }

    private async Task<bool> SetShopProductPriceForItemAsync(IImportProductItem item, long shopProductId)
    {
        var shopProductPrice = await _productDataService.GetShopProductPrice(shopProductId);
        if (shopProductPrice == null)
        {
            var currency = await _productDataService.GetCurrencyByName(item.Currency.Name)
                ?? await _productDataService.CreateCurrency(new Currency { Name = item.Currency.Name });

            await _productDataService.CreateShopProductPrice(new ShopProductPrice
            {
                ShopProductId = shopProductId,
                Price = item.ShopProductPrice.Price,
                CurrencyId = currency.Id
            });
            return await Task.FromResult(true);
        }
        else
        {
            shopProductPrice.Price = item.ShopProductPrice.Price;
            var result = await _productDataService.UpdateShopProductPrice(shopProductPrice);
            return await Task.FromResult(result);
        }
    }

    private async Task<IProduct> CreateProductFromModelAsync(IImportProductItem productItem, int shopId)
    {
        var brand = !string.IsNullOrWhiteSpace(productItem.Brand.Name) ? await GetBrandAsync(productItem) : null;

        var productType = await _productDataService.FindProductTypeByName(productItem.ProductType.Name) ??
            await _productDataService.CreateProductType(new ProductType { Name = productItem.ProductType.Name });

        var product = await _productDataService.CreateProduct(new Entities.Product
        {
            Name = productItem.Product.Name,
            BrandId = brand?.Id,
            ProductTypeId = productType.Id,
            InitShopId = shopId,
            AddedTime = DateTime.UtcNow,
            Articul = productItem.Product.Articul
        });

        if (productItem.PurposeTypes.Any())
            await SetProductPurposesAsync(productItem.PurposeTypes, product.Id);

        if (productItem.Components != null)
            await SetProductComponentsAsync(productItem.Components, product.Id);

        return await Task.FromResult(product);
    }

    private async Task SetProductComponentsAsync(IEnumerable<IComponent> components, long productId)
    {
        int componentNumber = 0;
        foreach (var itemComponent in components)
        {
            var componentName = itemComponent.Name.Trim().RemoveSpecialCharacters();
            var component = await _productDataService.FindComponentByName(componentName) ??
                await _productDataService.CreateComponent(new Component { Name = componentName });

            componentNumber++;

            var productComponent = _productDataService.SetProductComponent(new ProductComponent { ProductId = productId, ComponentId = component.Id, SequalNumber = (short)componentNumber });
        }
    }

    private async Task SetProductPurposesAsync(IEnumerable<IPurposeType> purposes, long productId)
    {
        var productPurposes = await _productDataService.GetProductPurposes(productId);
        foreach (var purpose in purposes.Where(p => 
        !productPurposes.Any(pp => pp.Name.Equals(p.Name.Trim(), StringComparison.InvariantCultureIgnoreCase))))
        {
            var name = purpose.Name.Trim().RemoveSpecialCharacters();

            var newPurposeType = await _productDataService.FindPurposeTypeByName(name) ??
                await _productDataService.CreatePurposeType(new PurposeType { Name = name });

            await _productDataService.SetProductPurpose(new ProductPurpose { ProductId = productId, PurposeTypeId = newPurposeType.Id });
        }
    }

    private async Task<IBrand?> GetBrandAsync(IImportProductItem productItem)
    {
        var brand = await _productDataService.FindBrandByName(productItem.Brand.Name);
        if (brand == null)
        {
            var country = !string.IsNullOrEmpty(productItem.Country.Name) ?
            (await _productDataService.FindCountryByName(productItem.Country.Name)
                ?? await _productDataService.CreateCountry(new Country { Name = productItem.Country.Name.Trim().RemoveSpecialCharacters() }))
                : null;

            brand = await _productDataService.CreateBrand(new Brand() { CountryId = country?.Id, Name = productItem.Brand.Name.Trim() });
        }

        return brand;
    }

    async Task<ItemProcessStatus> IItemHandler.HandleItem(object item)
    {
        if (item is not IImportProductItem productItem)
            throw new InvalidOperationException($"Invalid item type {item.GetType().Name}. Must be {nameof(IImportProductItem)}.");

        return await HandleItem(productItem);
    }
}
