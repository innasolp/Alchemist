using Alchemist.Common;
using Alchemist.Product.DataService.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Product.Interfaces;
using System.ComponentModel;
using Alchemist.Import.Shop.Interfaces;

namespace Alchemist.Import.Products.Data;

internal class ProductDataHandler(IProductDataService alchemyServiceClient) : IProductDataHandler
{
    private readonly IProductDataService _alchemyServiceClient = alchemyServiceClient;
    public async Task<ItemProcessStatus> HandleItem(IProductItem productItem,IShopUrlModel shopUrlModel)
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
            var result = await SetShopProductPriceForItemAsync(productItem, shopProduct);
            if (!result)
                throw new WarningException($"Price for shop product {shopProduct.Id} was not set. Url {productItem.ApiUrl}");               
            return !result ? ItemProcessStatus.Error : ItemProcessStatus.Updated;
        }

        var product = (await _alchemyServiceClient.FindProductByNameAndBrand(productItem?.Name, productItem?.Brand)
                            ?? await _alchemyServiceClient.FindProductByName(productItem?.Name))
            ?? await CreateProductFromModelAsync(productItem, shopUrlModel.ShopId);

        shopProduct.ProductId = product.Id;
        shopProduct.IsActual = true;
        var newShopProduct = await _alchemyServiceClient.CreateShopProduct(shopProduct);
        if (!await SetShopProductPriceForItemAsync(productItem, newShopProduct))
            throw new WarningException($"Price for shop product {shopProduct.Id} was not set. Url {productItem.ApiUrl}");

        return ItemProcessStatus.New;
    }

    private async Task<bool> SetShopProductPriceForItemAsync(IProductItem item, ShopProduct shopProduct)
    {
        var shopProductPrice = await _alchemyServiceClient.GetShopProductPrice(shopProduct.Id);
        if (shopProductPrice == null)
        {
            var currency = await _alchemyServiceClient.GetCurrencyByName(item.Currency)
                ?? await _alchemyServiceClient.CreateCurrency(new Currency { Name = item.Currency });

            await _alchemyServiceClient.CreateShopProductPrice(new ShopProductPrice
            {
                ShopProductId = shopProduct.Id,
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
        Brand? brand = null;
        if (!string.IsNullOrWhiteSpace(shopProductModel.Brand))
        {
            brand = await _alchemyServiceClient.FindBrandByName(shopProductModel.Brand);
            if (brand == null)
            {
                var country = await _alchemyServiceClient.FindCountryByName(shopProductModel.Country)
                    ?? await _alchemyServiceClient.CreateCountry(new Country { Name = shopProductModel.Country, Transcript = shopProductModel.Country });

                brand = await _alchemyServiceClient.CreateBrand(new Brand() { CountryId = country.Id, Name = shopProductModel.Brand });
            }
        }

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
        {
            int componentNumber = 0;
            foreach (var itemComponent in shopProductModel.Components)
            {
                var component = await _alchemyServiceClient.FindComponentByName(itemComponent) ??
                    await _alchemyServiceClient.CreateComponent(new Product.Entities.Component { Name = itemComponent });

                componentNumber++;

                var productComponent = _alchemyServiceClient.SetProductComponent(new ProductComponent { ProductId = product.Id, ComponentId = component.Id, SequalNumber = (short)componentNumber });
            }
        }

        return await Task.FromResult(product);
    }

    async Task<ItemProcessStatus> IItemHandler.HandleItem(object item,IShopUrlModel shopUrlModel)
    {
        if (item is not IProductItem productItem)
            return await Task.FromResult(ItemProcessStatus.Error);

        return await HandleItem(productItem, shopUrlModel);
    }
}
