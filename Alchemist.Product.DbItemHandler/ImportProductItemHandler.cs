using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Exceptions;
using Alchemist.Product.Entities;
using Alchemist.Product.ImportItem.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.DbItemHandler;

internal class ImportProductItemHandler(IProductDataService productDataService, IShopDataService shopDataService, string eventName, IShopCache shopCache)
    : IImportItemHandler
{
    private class ImportProductProcessEventArgs(IProductData item, ItemProcessStatus processStatus, CancellationToken cancellationToken)
       : ItemProcessEventArgs<object, ItemProcessStatus>(item, processStatus, cancellationToken)
    { }

    private readonly IProductDataService _productDataService = productDataService;

    private readonly IShopDataService _shopDataService = shopDataService;

    private readonly IShopCache _shopCache = shopCache;

    public string EventName { get; private set; } = eventName;

    Type IImportItemHandler.ItemType => typeof(ProductData);

    private event AsyncEventHandler<ItemProcessEventArgs<object, ItemProcessStatus>>? _itemProcessed;
    public event AsyncEventHandler<ItemProcessEventArgs<object, ItemProcessStatus>> ItemProcessed
    {
        add => _itemProcessed += value;
        remove => _itemProcessed -= value;
    }

    public async Task<ItemProcessStatus> HandleItem(object item, CancellationToken cancellationToken = default)
    {
        if (item is not IProductData productData)
            throw new InvalidDataException($"Item type {item.GetType().Name} is invalid. Expected type must implement {nameof(IProductData)}");

        try
        {
            var shop = await _shopCache.TryGetShopAsync(productData.ShopName, productData.ShopUrl)
            ?? throw new InvalidDataException($"Shop with name {productData.ShopName} or url {productData.ShopUrl} not found.");

            var shopProduct = await _productDataService.GetShopProductByShopAndItemId(shop.Id, productData.ShopProduct?.ItemId ?? string.Empty)
                ??
                new ShopProduct
                {
                    ShopId = shop.Id,
                    ItemId = productData.ShopProduct.ItemId,
                    ApiUrl = productData.ShopProduct.ApiUrl,
                    ItemUrl = productData.ShopProduct.ItemUrl
                };

            if (shopProduct.ProductId != 0)
            {
                var setCategoryResult = await SetShopProductCategoryIfNeedAsync(shop.Id, shopProduct.Id, productData.ShopCategory.ItemId);
                //todo
                //if(!categoryResult)
                //    throw new WarningException($"Category {productItem.CategoryId} in shop {shopUrlModel.ShopName} not found. Url {productItem.ApiUrl}");

                var setPriceResult = await SetShopProductPriceForItemAsync(productData, shopProduct.Id);
                //todo
                //if (!result)
                //    throw new WarningException($"Price for shop product {shopProduct.Id} was not set. Url {productItem.ApiUrl}");            

                var result = !(setPriceResult & setCategoryResult) ? ItemProcessStatus.Error : ItemProcessStatus.Updated;

                return result;
            }

            var product = await _productDataService.FindProductByNameAndBrand(productData.Product.Name, productData.Brand?.Name)
                                ?? await _productDataService.FindProductByName(productData.Product.Name);

            if (product != null)
            {
                if (await _productDataService.GetShopProductByShopAndProductId(shop.Id, product.Id) != null)
                {
                    await InvokeItemProcessedAsync(productData, ItemProcessStatus.AlreadyExists, cancellationToken);
                    return ItemProcessStatus.AlreadyExists;
                }
            }
            else
                product = await CreateProductFromModelAsync(productData, shop.Id);

            shopProduct.ProductId = product.Id;
            shopProduct.IsActual = true;

            var newShopProduct = await _productDataService.CreateShopProduct(shopProduct);

            if (!await SetShopProductPriceForItemAsync(productData, newShopProduct.Id) ||
                    !await SetShopProductCategoryIfNeedAsync(shop.Id, newShopProduct.Id, productData.ShopCategory.ItemId))
            {
                await InvokeItemProcessedAsync(productData, ItemProcessStatus.Error, cancellationToken);
                return await Task.FromResult(ItemProcessStatus.Error);
            }

            //todo    throw new WarningException($"Price for shop product {shopProduct.Id} was not set. Url {productItem.ApiUrl}");

            await InvokeItemProcessedAsync(productData, ItemProcessStatus.New, cancellationToken);
            return ItemProcessStatus.New;
        }
        catch (Exception e)
        {
            throw new WarningException($"Product {productData.ShopProduct.ItemUrl} proccessed with error.", e);
        }
    }

    private Task InvokeItemProcessedAsync(IProductData item, ItemProcessStatus itemProcessStatus, CancellationToken cancellationToken)
    {
        return _itemProcessed?.Invoke(this, new ImportProductProcessEventArgs(item, itemProcessStatus, cancellationToken)) ?? Task.FromResult(false);
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

    private async Task<bool> SetShopProductPriceForItemAsync(IProductData item, long shopProductId)
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

    private async Task<IProduct> CreateProductFromModelAsync(IProductData productItem, int shopId)
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

    private async Task<IBrand?> GetBrandAsync(IProductData productItem)
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
}