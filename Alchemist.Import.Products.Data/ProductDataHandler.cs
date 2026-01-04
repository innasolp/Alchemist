using System;
using System.Linq;
using System.Threading;
using Alchemist.Common;
using Alchemist.Product.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.DataService.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Import.Interfaces;
using System.ComponentModel;
using Component = Alchemist.Product.Entities.Component;

namespace Alchemist.Import.Products.Data;

internal class ProductDataHandler(IProductDataService alchemyServiceClient, IShopDataService shopDataService) : IProductItemHandler
{
    private readonly IProductDataService _alchemyServiceClient = alchemyServiceClient;

    private readonly IShopDataService _shopDataService = shopDataService;

    public event AsyncItemHandler<IProductItem> ItemProcessed;

    public async Task<ItemProcessStatus> HandleItem(IProductItem productItem, IShopModel shopModel, CancellationToken cancellationToken = default)
    {
        try
        {
            //todo
            var shop = shopModel as IShop;
            var shopId = shop.Id;

            var shopProduct = await _alchemyServiceClient.GetShopProductByShopAndItemId(shopId, productItem.ItemId, cancellationToken)
                ??
                new ShopProduct
                {
                    ShopId = shopId,
                    ItemId = productItem.ItemId,
                    ApiUrl = productItem.ApiUrl,
                    ItemUrl = productItem.Url
                };

            if (shopProduct.ProductId != 0)
            {
                var setCategoryResult = await SetShopProductCategoryIfNeedAsync(shopId, shopProduct.Id, productItem.CategoryId, cancellationToken);
                //todo
                //if(!categoryResult)
                //    throw new WarningException($"Category {productItem.CategoryId} in shop {shopUrlModel.ShopName} not found. Url {productItem.ApiUrl}");

                var setPriceResult = await SetShopProductPriceForItemAsync(productItem, shopProduct.Id, cancellationToken);
                //todo
                //if (!result)
                //    throw new WarningException($"Price for shop product {shopProduct.Id} was not set. Url {productItem.ApiUrl}");            

                var result = !(setPriceResult & setCategoryResult) ? ItemProcessStatus.Error : ItemProcessStatus.Updated;

                await InvokeItemProcessedAsync(productItem, shopModel, result);

                return result;
            }

            var product = await _alchemyServiceClient.FindProductByNameAndBrand(productItem?.Name, productItem?.Brand, CancellationToken.None)
                                ?? await _alchemyServiceClient.FindProductByName(productItem?.Name, CancellationToken.None);

            if (product != null)
            {
                if (await _alchemyServiceClient.GetShopProductByShopAndProductId(shopId, product.Id, CancellationToken.None) != null)
                    return ItemProcessStatus.AlreadyExists;
            }
            else
                product = await CreateProductFromModelAsync(productItem, shopId);

            shopProduct.ProductId = product.Id;
            shopProduct.IsActual = true;

            var newShopProduct = await _alchemyServiceClient.CreateShopProduct(shopProduct, CancellationToken.None);

            if (!await SetShopProductPriceForItemAsync(productItem, newShopProduct.Id) ||
                    !await SetShopProductCategoryIfNeedAsync(shopId, newShopProduct.Id, productItem.CategoryId))
                return await Task.FromResult(ItemProcessStatus.Error);

            //todo    throw new WarningException($"Price for shop product {shopProduct.Id} was not set. Url {productItem.ApiUrl}");
            await InvokeItemProcessedAsync(productItem, shopModel, ItemProcessStatus.New);

            return ItemProcessStatus.New;
        }
        catch(Exception e)
        {
            await InvokeItemProcessedAsync(productItem, shopModel, ItemProcessStatus.Warning);

            throw new WarningException($"Product {productItem.Url} proccessed with error.", e);
        }
    }

    private Task InvokeItemProcessedAsync(IProductItem productItem, IShopModel shopModel, ItemProcessStatus itemProcessStatus)
    {
        return ItemProcessed?.Invoke(this, productItem, shopModel, itemProcessStatus) ?? Task.FromResult(false);
    }

    private async Task<bool> SetShopProductCategoryIfNeedAsync(int shopId, long shopProductId, int categoryItemId, CancellationToken cancellationToken = default)
    {
        var category = await _shopDataService.GetShopCategoryByShopIdAndItemId(shopId, categoryItemId, cancellationToken);
        if (category == null)
            return await Task.FromResult(false);

        var categoryExists = await _alchemyServiceClient.CheckShopProductCategory(shopProductId, category.Id, cancellationToken);
        if (!categoryExists)
            await _alchemyServiceClient.AddShopProductCategory(shopProductId, category.Id, cancellationToken);

        return await Task.FromResult(true);
    }

    private async Task<bool> SetShopProductPriceForItemAsync(IProductItem item, long shopProductId, CancellationToken cancellationToken = default)
    {
        var shopProductPrice = await _alchemyServiceClient.GetShopProductPrice(shopProductId, cancellationToken);
        if (shopProductPrice == null)
        {
            var currency = await _alchemyServiceClient.GetCurrencyByName(item.Currency, cancellationToken)
                ?? await _alchemyServiceClient.CreateCurrency(new Currency { Name = item.Currency }, cancellationToken);

            await _alchemyServiceClient.CreateShopProductPrice(new ShopProductPrice
            {
                ShopProductId = shopProductId,
                Price = item.Price,
                CurrencyId = currency.Id
            }, cancellationToken);
            return await Task.FromResult(true);
        }
        else
        {
            shopProductPrice.Price = item.Price;
            var result = await _alchemyServiceClient.UpdateShopProductPrice(shopProductPrice, cancellationToken);
            return await Task.FromResult(result);
        }
    }

    private async Task<IProduct> CreateProductFromModelAsync(IProductItem productItem, int shopId, CancellationToken cancellationToken = default)
    {
        var brand = !string.IsNullOrWhiteSpace(productItem.Brand) ? await GetBrandAsync(productItem, cancellationToken) : null;

        var productType = await _alchemyServiceClient.FindProductTypeByName(productItem.ProductType, cancellationToken) ??
            await _alchemyServiceClient.CreateProductType(new ProductType { Name = productItem.ProductType }, cancellationToken);

        var product = await _alchemyServiceClient.CreateProduct(new Product.Entities.Product
        {
            Name = productItem.Name,
            BrandId = brand?.Id,
            ProductTypeId = productType.Id,
            InitShopId = shopId,
            AddedTime = DateTime.UtcNow,
            Articul = productItem.Articul
        }, CancellationToken.None);

        if (productItem.Purposes.Length > 0)
            await SetProductPurposesAsync(productItem.Purposes, product.Id, cancellationToken);

        if (productItem.Components != null)
            await SetProductComponentsAsync(productItem.Components, product.Id, cancellationToken);

        return await Task.FromResult(product);
    }

    private async Task SetProductComponentsAsync(IEnumerable<string> components, long productId, CancellationToken cancellationToken = default)
    {
        int componentNumber = 0;
        foreach (var itemComponent in components)
        {
            var componentName = itemComponent.Trim().RemoveSpecialCharacters();
            var component = await _alchemyServiceClient.FindComponentByName(componentName, cancellationToken) ??
                await _alchemyServiceClient.CreateComponent(new Component { Name = componentName }, cancellationToken);

            componentNumber++;

            await _alchemyServiceClient.SetProductComponent(new ProductComponent { ProductId = productId, ComponentId = component.Id, SequalNumber = (short)componentNumber }, cancellationToken);
        }
    }

    private async Task SetProductPurposesAsync(IEnumerable<string> purposes, long productId, CancellationToken cancellationToken = default)
    {
        var productPurposes = await _alchemyServiceClient.GetProductPurposes(productId, cancellationToken);
        foreach(var purpose in purposes.Where(p=>!productPurposes.Any(pp=>pp.Name.Equals(p.Trim(), StringComparison.InvariantCultureIgnoreCase))))
        {
            var name = purpose.Trim().RemoveSpecialCharacters();

            var newPurposeType = await _alchemyServiceClient.FindPurposeTypeByName(name, cancellationToken) ??
                await _alchemyServiceClient.CreatePurposeType(new PurposeType { Name = name }, cancellationToken);

            await _alchemyServiceClient.SetProductPurpose(new ProductPurpose { ProductId = productId, PurposeTypeId = newPurposeType.Id }, cancellationToken);
        }
    }

    private async Task<IBrand?> GetBrandAsync(IProductItem productItem, CancellationToken cancellationToken = default)
    {
        var brand = await _alchemyServiceClient.FindBrandByName(productItem.Brand, cancellationToken);
        if (brand == null)
        {
            var country = !string.IsNullOrEmpty(productItem.Country) ?
            (await _alchemyServiceClient.FindCountryByName(productItem.Country, cancellationToken)
                ?? await _alchemyServiceClient.CreateCountry(new Country { Name = productItem.Country.Trim().RemoveSpecialCharacters() }, cancellationToken))
                : null;

            brand = await _alchemyServiceClient.CreateBrand(new Brand() { CountryId = country?.Id, Name = productItem.Brand.Trim() }, cancellationToken);
        }

        return brand;
    }

    async Task<ItemProcessStatus> IItemHandler.HandleItem(object item, IShopModel shopModel, CancellationToken cancellationToken = default)
    {
        if (item is not IProductItem productItem)
            return await Task.FromResult(ItemProcessStatus.Error);

        return await HandleItem(productItem, shopModel, cancellationToken);
    }
}