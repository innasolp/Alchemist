using Alchemist.Common;
using Alchemist.Product.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.DataService.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Import.Interfaces;

namespace Alchemist.Import.Products.Data;

internal class ProductDataHandler(IProductDataService alchemyServiceClient, IShopDataService shopDataService) : IProductItemHandler
{
    private readonly IProductDataService _alchemyServiceClient = alchemyServiceClient;

    private readonly IShopDataService _shopDataService = shopDataService;
    public async Task<ItemProcessStatus> HandleItem(IProductItem productItem, IShopModel shopModel)
    {
        //todo
        var shop = shopModel as IShop;
        var shopId = shop.Id;

        var shopProduct = await _alchemyServiceClient.GetShopProductByShopAndItemId(shopId, productItem.ItemId)
            ??
            new ShopProduct
            {
                ShopId = shopId,
                ItemId = productItem.ItemId,
                ApiUrl = productItem.ApiUrl,
                ItemUrl = productItem.ItemUrl,
                LastUpdate = DateTime.Now
            };

        if (shopProduct.ProductId != 0)
        {
            var setCategoryResult = await SetShopProductCategoryIfNeedAsync(shopId, shopProduct.Id, productItem.CategoryId);
            //todo
            //if(!categoryResult)
            //    throw new WarningException($"Category {productItem.CategoryId} in shop {shopUrlModel.ShopName} not found. Url {productItem.ApiUrl}");

            var setPriceResult = await SetShopProductPriceForItemAsync(productItem, shopProduct.Id);
            //todo
            //if (!result)
            //    throw new WarningException($"Price for shop product {shopProduct.Id} was not set. Url {productItem.ApiUrl}");            

            return !(setPriceResult & setCategoryResult) ? ItemProcessStatus.Error : ItemProcessStatus.Updated;
        }

        var product = await _alchemyServiceClient.FindProductByNameAndBrand(productItem?.Name, productItem?.Brand)
                            ?? await _alchemyServiceClient.FindProductByName(productItem?.Name);

        if (product != null)
        {
            if (await _alchemyServiceClient.GetShopProductByShopAndProductId(shopId, product.Id) != null)
                return ItemProcessStatus.AlreadyExists;
        }
        else
            product = await CreateProductFromModelAsync(productItem, shopId);

        shopProduct.ProductId = product.Id;
        shopProduct.IsActual = true;

        var newShopProduct = await _alchemyServiceClient.CreateShopProduct(shopProduct);

        if (!await SetShopProductPriceForItemAsync(productItem, newShopProduct.Id) ||
                !await SetShopProductCategoryIfNeedAsync(shopId, newShopProduct.Id, productItem.CategoryId))
            return await Task.FromResult(ItemProcessStatus.Error);

        //todo    throw new WarningException($"Price for shop product {shopProduct.Id} was not set. Url {productItem.ApiUrl}");

        return ItemProcessStatus.New;
    }

    private async Task<bool> SetShopProductCategoryIfNeedAsync(int shopId, long shopProductId, int categoryItemId)
    {
        var category = await _shopDataService.GetShopCategoryByShopIdAndItemId(shopId, categoryItemId);
        if (category == null)
            return await Task.FromResult(false);

        var categoryExists = await _alchemyServiceClient.CheckShopProductCategory(shopProductId, category.Id);
        if (!categoryExists)
            await _alchemyServiceClient.AddShopProductCategory(shopProductId, category.Id);

        return await Task.FromResult(true);
    }

    private async Task<bool> SetShopProductPriceForItemAsync(IProductItem item, long shopProductId)
    {
        var shopProductPrice = await _alchemyServiceClient.GetShopProductPrice(shopProductId);
        if (shopProductPrice == null)
        {
            var currency = await _alchemyServiceClient.GetCurrencyByName(item.Currency)
                ?? await _alchemyServiceClient.CreateCurrency(new Currency { Name = item.Currency });

            await _alchemyServiceClient.CreateShopProductPrice(new ShopProductPrice
            {
                ShopProductId = shopProductId,
                Price = item.Price,
                CurrencyId = currency.Id,
                LastUpdate = DateTime.Now
            });
            return await Task.FromResult(true);
        }
        else
        {
            shopProductPrice.Price = item.Price;
            shopProductPrice.LastUpdate = DateTime.Now;
            var result = await _alchemyServiceClient.UpdateShopProductPrice(shopProductPrice);
            return await Task.FromResult(result);
        }
    }

    private async Task<IProduct> CreateProductFromModelAsync(IProductItem productItem, int shopId)
    {
        var brand = !string.IsNullOrWhiteSpace(productItem.Brand) ? await GetBrandAsync(productItem) : null;

        var productType = await _alchemyServiceClient.FindProductTypeByName(productItem.ProductType) ??
            await _alchemyServiceClient.CreateProductType(new ProductType { Name = productItem.ProductType });

        var purposeTypes = new List<IPurposeType>();
        foreach (var purpose in productItem.Purposes)
        {
            var purposeType = await _alchemyServiceClient.FindPurposeTypeByName(purpose) ??
                await _alchemyServiceClient.CreatePurposeType(new PurposeType { Name = purpose });
            purposeTypes.Add(purposeType);
        }

        var product = await _alchemyServiceClient.CreateProduct(new Product.Entities.Product
        {
            Name = productItem.Name,
            BrandId = brand?.Id,
            ProductTypeId = productType.Id,
            InitShopId = shopId,
            AddedTime = DateTime.UtcNow,
            Articul = productItem.Articul
        });

        if (productItem.Components != null)
            await SetProductComponentsAsync(productItem.Components, product.Id);

        return await Task.FromResult(product);
    }

    private async Task SetProductComponentsAsync(IEnumerable<string> components, long productId)
    {
        int componentNumber = 0;
        foreach (var itemComponent in components)
        {
            var componentName = itemComponent.RemoveSpecialCharacters();
            var component = await _alchemyServiceClient.FindComponentByName(componentName) ??
                await _alchemyServiceClient.CreateComponent(new Component { Name = componentName });

            componentNumber++;

            var productComponent = _alchemyServiceClient.SetProductComponent(new ProductComponent { ProductId = productId, ComponentId = component.Id, SequalNumber = (short)componentNumber });
        }
    }

    private async Task<IBrand?> GetBrandAsync(IProductItem shopProductModel)
    {
        var brand = await _alchemyServiceClient.FindBrandByName(shopProductModel.Brand);
        if (brand == null)
        {
            var country = !string.IsNullOrEmpty(shopProductModel.Country) ?
            (await _alchemyServiceClient.FindCountryByName(shopProductModel.Country)
                ?? await _alchemyServiceClient.CreateCountry(new Country { Name = shopProductModel.Country.RemoveSpecialCharacters() }))
                : null;

            brand = await _alchemyServiceClient.CreateBrand(new Brand() { CountryId = country?.Id, Name = shopProductModel.Brand });
        }

        return brand;
    }

    async Task<ItemProcessStatus> IItemHandler.HandleItem(object item, IShopModel shopModel)
    {
        if (item is not IProductItem productItem)
            return await Task.FromResult(ItemProcessStatus.Error);

        return await HandleItem(productItem, shopModel);
    }
}
