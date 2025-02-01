using Alchemist.Common;
using Alchemist.Product.DataService.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Shop.Interfaces;

namespace Alchemist.Import.Products.Data;

internal class ProductDataHandler(IProductDataService alchemyServiceClient, IShopDataService shopDataService) : IProductDataHandler
{
    private readonly IProductDataService _alchemyServiceClient = alchemyServiceClient;

    private readonly IShopDataService _shopDataService = shopDataService;
    public async Task<ItemProcessStatus> HandleItem(IProductItem productItem,IShopModel shopUrlModel)
    {
        var shopProduct = await _alchemyServiceClient.GetShopProductByShopAndItemId(shopUrlModel.ShopId, productItem.ItemId)
            ??
            new ShopProduct
            {
                ShopId = shopUrlModel.ShopId,
                ItemId = productItem.ItemId,
                ApiUrl = productItem.ApiUrl,
                ItemUrl = productItem.ItemUrl,
                LastUpdate = DateTime.Now
            };

        if (shopProduct.ProductId != 0)
        {
            var setCategoryResult = await SetShopProductCategoryIfNeedAsync(shopUrlModel.ShopId, shopProduct.Id, productItem.CategoryId);
            //todo
            //if(!categoryResult)
            //    throw new WarningException($"Category {productItem.CategoryId} in shop {shopUrlModel.ShopName} not found. Url {productItem.ApiUrl}");

            var setPriceResult = await SetShopProductPriceForItemAsync(productItem, shopProduct.Id);
            //todo
            //if (!result)
            //    throw new WarningException($"Price for shop product {shopProduct.Id} was not set. Url {productItem.ApiUrl}");            
            
            return !(setPriceResult & setCategoryResult) ? ItemProcessStatus.Error : ItemProcessStatus.Updated;
        }

        var product = (await _alchemyServiceClient.FindProductByNameAndBrand(productItem?.Name, productItem?.Brand)
                            ?? await _alchemyServiceClient.FindProductByName(productItem?.Name))
            ?? await CreateProductFromModelAsync(productItem, shopUrlModel.ShopId);

        shopProduct.ProductId = product.Id;
        shopProduct.IsActual = true;

        var newShopProduct = await _alchemyServiceClient.CreateShopProduct(shopProduct);

        if (!await SetShopProductPriceForItemAsync(productItem, newShopProduct.Id) ||
                !await SetShopProductCategoryIfNeedAsync(shopUrlModel.ShopId, newShopProduct.Id, productItem.CategoryId))
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

    private async Task<Product.Entities.Product> CreateProductFromModelAsync(IProductItem shopProductModel, int shopId)
    {
        Brand? brand = !string.IsNullOrWhiteSpace(shopProductModel.Brand) ? await GetBrandAsync(shopProductModel) : null;       

        var productType = await _alchemyServiceClient.FindProductTypeByName(shopProductModel.ProductType) ??
            await _alchemyServiceClient.CreateProductType(new ProductType { Name = shopProductModel.ProductType });

        var purposeTypes = new List<PurposeType>();
        foreach (var purpose in shopProductModel.Purposes)
        {
            var purposeType = await _alchemyServiceClient.FindPurposeTypeByName(purpose) ??
                await _alchemyServiceClient.CreatePurposeType(new PurposeType { Name = purpose });
            purposeTypes.Add(purposeType);
        }

        var product = await _alchemyServiceClient.CreateProduct(new Product.Entities.Product
        {
            Name = shopProductModel.Name,
            BrandId = brand?.Id,
            ProductTypeId = productType.Id,
            InitShopId = shopId,
            AddedTime = DateTime.UtcNow,
            Articul = shopProductModel.Articul
        });

        if (shopProductModel.Components != null)        
            await SetProductComponentsAsync(shopProductModel.Components, product.Id);        

        return await Task.FromResult(product);
    }

    private async Task SetProductComponentsAsync(IEnumerable<string> components, long productId)
    {
        int componentNumber = 0;
        foreach (var itemComponent in components)
        {
            var component = await _alchemyServiceClient.FindComponentByName(itemComponent) ??
                await _alchemyServiceClient.CreateComponent(new Product.Entities.Component { Name = itemComponent });

            componentNumber++;

            var productComponent = _alchemyServiceClient.SetProductComponent(new ProductComponent { ProductId = productId, ComponentId = component.Id, SequalNumber = (short)componentNumber });
        }
    }

    private async Task<Brand?> GetBrandAsync(IProductItem shopProductModel)
    {
        var brand = await _alchemyServiceClient.FindBrandByName(shopProductModel.Brand);
        if (brand == null)
        {
            var country = await _alchemyServiceClient.FindCountryByName(shopProductModel.Country)
                ?? await _alchemyServiceClient.CreateCountry(new Country { Name = shopProductModel.Country, Transcript = shopProductModel.Country });

            brand = await _alchemyServiceClient.CreateBrand(new Brand() { CountryId = country.Id, Name = shopProductModel.Brand });
        }

        return brand;
    }

    async Task<ItemProcessStatus> IItemHandler.HandleItem(object item,IShopModel shopUrlModel)
    {
        if (item is not IProductItem productItem)
            return await Task.FromResult(ItemProcessStatus.Error);

        return await HandleItem(productItem, shopUrlModel);
    }
}
