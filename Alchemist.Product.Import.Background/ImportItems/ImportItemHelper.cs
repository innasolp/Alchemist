using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Product.DataItem.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Background.ImportItems;

public static class ImportItemHelper
{
    public static ImportItem.Interfaces.IProductData ConvertToImportProductItem(this IProductItem productItem, IShopItem shopItem)
    {
        //todo
        if (shopItem is not IShop shop)
            return null;

        return new ProductData
        {
            Product = new Entities.Product { Name = productItem.Name, Articul = productItem.Articul},
            ShopProduct = new ShopProduct { ApiUrl = productItem.ApiUrl, ItemUrl = productItem.ItemId, ShopId = shop.Id, ItemId = productItem.ItemId },
            ShopProductPrice = new ShopProductPrice {  Price = productItem.Price },
            Brand = new Brand { Name = productItem.Brand},
            Country = !string.IsNullOrEmpty(productItem.Country) ? new Country {  Name = productItem.Country } : null,
            Currency = new Currency {  Name = productItem.Currency },
            ProductType = new ProductType {  Name =  productItem.ProductType },
            PurposeTypes =  productItem.Purposes.Select(p=>new PurposeType { Name = p}),
            Components = productItem.Components?.Select(c=>new Component { Name = c}) ?? [],
            ShopCategory = new ShopCategory { ItemId = productItem.CategoryId },
            ShopId = shop.Id
        };
    }

    public static ICategoryData ConvertToImportCategoryItem(this ICategory categoryItem, IShopItem shopItem)
    {
        //todo
        if (shopItem is not IShop shop)
            return null;

        return new CategoryData
        {
            ParentCategory = categoryItem.ItemParent != null
                ? new ShopCategory { Category = categoryItem.ItemParent.Name, ItemId = categoryItem.ItemParent.Id, ShopId = shop.Id }
                : null,
            ShopCategory = new ShopCategory { Category = categoryItem.Name, ItemId = categoryItem.Id, ShopId = shop.Id },
            ShopId = shop.Id
        };
    }
}
