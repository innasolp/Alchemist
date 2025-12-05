using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Product.DataItem.Interfaces;
using Alchemist.Product.Entities;
namespace Alchemist.Product.ImportItemHandler;

internal static class ImportItemHelper
{
    public static ImportItem.Interfaces.IProductData ConvertToImportProductItem(this IProductItem productItem, string shopName, string shopUrl)
    {
        return new ProductData
        {
            Product = new Entities.Product { Name = productItem.Name, Articul = productItem.Articul},
            ShopProduct = new ShopProduct { ApiUrl = productItem.ApiUrl, ItemUrl = productItem.ItemId, ItemId = productItem.ItemId },
            ShopProductPrice = new ShopProductPrice {  Price = productItem.Price },
            Brand = new Brand { Name = productItem.Brand},
            Country = !string.IsNullOrEmpty(productItem.Country) ? new Country {  Name = productItem.Country } : null,
            Currency = new Currency {  Name = productItem.Currency },
            ProductType = new ProductType {  Name =  productItem.ProductType },
            PurposeTypes =  productItem.Purposes.Select(p=>new PurposeType { Name = p}),
            Components = productItem.Components?.Select(c=>new Component { Name = c}) ?? [],
            ShopCategory = new ShopCategory { ItemId = productItem.CategoryId },
           ShopName = shopName,
           ShopUrl = shopUrl,
        };
    }

    public static ICategoryData ConvertToImportCategoryItem(this ICategory categoryItem,string shopName, string shopUrl)
    {
        return new CategoryData
        {
            ParentCategory = categoryItem.ItemParent != null
                ? new ShopCategory { Category = categoryItem.ItemParent.Name, ItemId = categoryItem.ItemParent.Id }
                : null,
            ShopCategory = new ShopCategory { Category = categoryItem.Name, ItemId = categoryItem.Id },
            ShopName = shopName,
            ShopUrl = shopUrl
        };
    }
}