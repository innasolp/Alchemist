using Alchemist.Common;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Db.Infrastructure;
using Shop.Import.Common;
using Shop.Interfaces;

namespace Alchemist.Product.BeautyAndHealth.Commands;

public class ImportBeautyAndHealthProductCommandHandler(IProductDataService productDataService, 
    IShopDataService shopDataService,
    IShopCachedRepository shopCache) : ICommandHandler<ImportBeautyAndHealthProductCommand, ItemProcessStatus>, ICommandHandler
{
    private readonly IProductDataService _productDataService = productDataService;

    private readonly IShopDataService _shopDataService = shopDataService;

    private readonly IShopCachedRepository _shopCache = shopCache;

    public async Task<ItemProcessStatus> Handle(ImportBeautyAndHealthProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var shop = await _shopCache.TryGetShopAsync(request.Entity.ShopName, request.Entity.ShopUrl, cancellationToken)
            ?? throw new InvalidDataException($"Shop with name {request.Entity.ShopName} or url {request.Entity.ShopUrl} not found.");

            var shopProduct = await _productDataService.GetShopProductByShopIdAndItemId(shop.Id, request.Entity.ShopProduct?.ItemId ?? string.Empty, cancellationToken)
                ??
                new ShopProduct
                {
                    ShopId = shop.Id,
                    ItemId = request.Entity.ShopProduct.ItemId,
                    ApiUrl = request.Entity.ShopProduct.ApiUrl,
                    ItemUrl = request.Entity.ShopProduct.ItemUrl
                };

            if (shopProduct.ProductId != 0)
            {
                await SetShopProductPriceForItemAsync(request.Entity, shopProduct.Id, cancellationToken);

                var setCategoryResult = await SetShopProductCategoryIfNeedAsync(shop.Id, shopProduct.Id, request.Entity.ShopCategory.ItemId, cancellationToken);
                //todo
                //if(!categoryResult)
                //    throw new WarningException($"Category {productItem.CategoryId} in shop {shopUrlModel.ShopName} not found. Url {productItem.ApiUrl}");

                //todo
                //if (!result)
                //    throw new WarningException($"Price for shop product {shopProduct.Id} was not set. Url {productItem.ApiUrl}");            

                var result = !setCategoryResult ? ItemProcessStatus.Error : ItemProcessStatus.Updated;

                return result;
            }

            var product = !string.IsNullOrEmpty(request.Entity.Brand?.Name)
                ? await _productDataService.FindProductByNameAndBrand(request.Entity.Product.Name, request.Entity.Brand.Name, cancellationToken)
                    ?? await _productDataService.FindProductByName(request.Entity.Product.Name, cancellationToken)
                : await _productDataService.FindProductByName(request.Entity.Product.Name, cancellationToken);

            if (product != null)
            {
                if (await _productDataService.GetShopProductByShopIdAndProductId(shop.Id, product.Id, cancellationToken) != null)                
                    return ItemProcessStatus.AlreadyExists;
                
            }
            else
                product = await CreateProductFromModelAsync(request.Entity, shop.Id, cancellationToken);

            shopProduct.ProductId = product.Id;
            shopProduct.IsActual = true;

            var newShopProduct = await _productDataService.CreateShopProduct(shopProduct, cancellationToken);

            var shopProductPrice = await SetShopProductPriceForItemAsync(request.Entity, newShopProduct.Id, cancellationToken);

            if (!await SetShopProductCategoryIfNeedAsync(shop.Id, newShopProduct.Id, request.Entity.ShopCategory.ItemId, cancellationToken))            
                return await Task.FromResult(ItemProcessStatus.Error);            

            //todo    throw new WarningException($"Price for shop product {shopProduct.Id} was not set. Url {productItem.ApiUrl}");

            return ItemProcessStatus.New;
        }
        catch (Exception e)
        {
            throw new Exception($"Product {request.Entity.ShopProduct.ItemUrl} proccessed with error.", e);
        }
    }

    private async Task<bool> SetShopProductCategoryIfNeedAsync(int shopId, long shopProductId, int categoryItemId, CancellationToken cancellationToken = default)
    {
        var category = await _shopDataService.GetShopCategoryByShopIdAndItemId(shopId, categoryItemId, cancellationToken);
        if (category == null)
            return await Task.FromResult(false);

        var categoryExists = await _productDataService.CheckShopProductCategory(shopProductId, category.Id, cancellationToken);
        if (!categoryExists)
            await _productDataService.AddShopProductCategory(shopProductId, category.Id, cancellationToken);

        return await Task.FromResult(true);
    }

    private async Task<IShopProductPrice> SetShopProductPriceForItemAsync(BeautyAndHealthProductData item, long shopProductId,
        CancellationToken cancellationToken = default)
    {
        var shopProductPrice = await _productDataService.GetShopProductPrice(shopProductId, cancellationToken);
        if (shopProductPrice == null)
        {
            var currency = await _productDataService.GetCurrencyByName(item.Currency.Name, cancellationToken)
                ?? await _productDataService.CreateCurrency(new Currency { Name = item.Currency.Name }, cancellationToken);

            return await _productDataService.CreateShopProductPrice(new ShopProductPrice
            {
                ShopProductId = shopProductId,
                Price = item.ShopProductPrice.Price,
                CurrencyId = currency.Id
            }, cancellationToken);
        }
        else
        {
            shopProductPrice.Price = item.ShopProductPrice.Price;
            var result = await _productDataService.UpdateShopProductPrice(shopProductPrice, cancellationToken);
            return await Task.FromResult(result);
        }
    }

    private async Task<IProduct> CreateProductFromModelAsync(BeautyAndHealthProductData productItem, int shopId, CancellationToken cancellationToken = default)
    {
        var brand = !string.IsNullOrWhiteSpace(productItem.Brand?.Name)
            ? await GetBrandAsync(productItem.Brand.Name, productItem.Country?.Name, cancellationToken)
            : null;

        var productType = await _productDataService.FindProductTypeByName(productItem.ProductType.Name, cancellationToken) ??
            await _productDataService.CreateProductType(new ProductType { Name = productItem.ProductType.Name }, cancellationToken);

        var product = await _productDataService.CreateProduct(new Entities.Product
        {
            Name = productItem.Product.Name,
            BrandId = brand?.Id,
            ProductTypeId = productType.Id,
            InitShopId = shopId,
            AddedTime = DateTime.UtcNow,
            Articul = productItem.Product.Articul
        }, cancellationToken);

        if (productItem.PurposeTypes.Any())
            await SetProductPurposesAsync(productItem.PurposeTypes, product.Id, cancellationToken);

        if (productItem.Components != null)
            await SetProductComponentsAsync(productItem.Components, product.Id, cancellationToken);

        return await Task.FromResult(product);
    }

    private async Task SetProductComponentsAsync(IEnumerable<IComponent> components, long productId, CancellationToken cancellationToken = default)
    {
        int componentNumber = 0;
        foreach (var itemComponent in components)
        {
            var componentName = itemComponent.Name.Trim().RemoveSpecialCharacters();
            var component = await _productDataService.FindComponentByName(componentName, cancellationToken) ??
                await _productDataService.CreateComponent(new Component { Name = componentName }, cancellationToken);

            componentNumber++;

            var productComponent = _productDataService.SetProductComponent(new ProductComponent { ProductId = productId, ComponentId = component.Id, SequalNumber = (short)componentNumber },
                cancellationToken);
        }
    }

    private async Task SetProductPurposesAsync(IEnumerable<IPurposeType> purposes, long productId, CancellationToken cancellationToken = default)
    {
        var productPurposes = await _productDataService.GetProductPurposes(productId, cancellationToken);
        foreach (var purpose in purposes.Where(p =>
        !productPurposes.Any(pp => pp.Name.Equals(p.Name.Trim(), StringComparison.InvariantCultureIgnoreCase))))
        {
            var name = purpose.Name.Trim().RemoveSpecialCharacters();

            var newPurposeType = await _productDataService.FindPurposeTypeByName(name, cancellationToken) ??
                await _productDataService.CreatePurposeType(new PurposeType { Name = name }, cancellationToken);

            await _productDataService.SetProductPurpose(new ProductPurpose { ProductId = productId, PurposeTypeId = newPurposeType.Id }, cancellationToken);
        }
    }

    private async Task<IBrand?> GetBrandAsync(string brandName, string? countryName, CancellationToken cancellationToken = default)
    {
        var brand = await _productDataService.FindBrandByName(brandName, cancellationToken);
        if (brand == null)
        {
            var country = !string.IsNullOrEmpty(countryName) ?
            (await _productDataService.FindCountryByName(countryName, cancellationToken)
                ?? await _productDataService.CreateCountry(new Country { Name = countryName.Trim().RemoveSpecialCharacters() }, cancellationToken))
                : null;

            brand = await _productDataService.CreateBrand(new Brand() { CountryId = country?.Id, Name = brandName.Trim() }, cancellationToken);
        }

        return brand;
    }

    Task ICommandHandler.Handle(object command, CancellationToken cancellationToken)
    {
        if (command is ImportBeautyAndHealthProductCommand importBeautyAndHealthCommand)
            return Handle(importBeautyAndHealthCommand, cancellationToken);

        throw new InvalidOperationException($"Command type is not {typeof(ImportBeautyAndHealthProductCommand).Name}.");
    }
}