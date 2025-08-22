using Alchemist.DataService.Interfaces;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Background.Models;

internal static class ShopModelExtensions
{
    internal static async Task<IShopItem> CreateShopModelAsync(this IShopDataService shopDataService, IShopImportSettings shopImportSettings)
    {
        return shopImportSettings.ShopSettingType == Alchemist.Import.Settings.Interfaces.ShopSettingType.Product
            ? await shopDataService.CreateProductShopModelAsync(shopImportSettings as IProductShopImportSettings) as IShopItem
            : await shopDataService.CreateCategoryShopModelAsync(shopImportSettings as ICategoryShopImportSettings);
    }

    private static async Task<IProductShopModel> CreateProductShopModelAsync(this IShopDataService shopDataService, IProductShopImportSettings shopImportSettings)
    {
        var productShopModel = await shopDataService.CreateShopModelCoreAsync(shopImportSettings
            , (shopName, shopUrl) => new ProductShopModel() { ShopName = shopName, ShopUrl = shopUrl, Host = new Uri(shopUrl).Host });

        productShopModel.ProductUrl = shopImportSettings.ProductUrlFormat;
        productShopModel.CategoryUrl = shopImportSettings.CategoryUrlFormat;        

        if (productShopModel.Id != 0)
        {
            if (shopImportSettings.RootCategories?.Length > 0)
            {
                var shopCategories = new List<IShopCategory>();

                foreach(var c in shopImportSettings.RootCategories)
                {
                    var shopCategory = await shopDataService.GetShopCategoryByShopIdAndItemId(productShopModel.Id, c.Item);
                    if (shopCategory != null)
                    {
                        var children = await shopDataService.GetAllCategoryChildren(shopCategory.Id);
                        var children2 = new List<IShopCategory>(children);
                        shopCategories.AddRange(children.Where(child => !children2.Any(c2 => c2.ParentId == child.Id)));
                    }
                }                

                if (shopCategories.Count > 0)
                    shopCategories?.ForEach(c => productShopModel.Categories.Add(new ProductShopCategoryModel { Category = c.Category, ItemId = c.ItemId }));
                else
                    shopImportSettings.RootCategories.ToList().ForEach(c => productShopModel.Categories.Add(new ProductShopCategoryModel { Category = c.Url, ItemId = c.Item }));
            }
            else
            {
                var allShopCategories = await shopDataService.GetShopCategories(productShopModel.Id);
                var categories2 = new List<IShopCategory>(allShopCategories);
                var lastShopCategories = allShopCategories.Where(c => !categories2.Any(c2 => c2.ParentId == c.Id)).ToList();
                lastShopCategories.ForEach(c => productShopModel.Categories.Add(new ProductShopCategoryModel { Category = c.Category, ItemId = c.ItemId }));
            }
        }
        else 
            shopImportSettings.RootCategories?.ToList().ForEach(c => productShopModel.Categories.Add(new ProductShopCategoryModel { Category = c.Url, ItemId = c.Item }));

        return productShopModel;
    }

    private static async Task<T> CreateShopModelCoreAsync<T>(this IShopDataService shopDataService, IShopImportSettings shopImportSettings, Func<string, string, T> createShopModel)
        where T: class, IShop, IShopItem
    {
        var shop = (shopImportSettings.Id != 0
                ? await shopDataService.GetShop(shopImportSettings.ShopId)
                : await shopDataService.GetShopByName(shopImportSettings.ShopName ?? shopImportSettings.Name)
                ?? await shopDataService.GetShopByUrl(shopImportSettings.ShopUrl))
                ?? await shopDataService.CreateShop(new Shop { Name = shopImportSettings.ShopName, Url = shopImportSettings.ShopUrl });

        var shopModel = createShopModel(shop?.Name ?? shopImportSettings.ShopName ?? shopImportSettings.Name,
            shop?.Url ?? shopImportSettings.ShopUrl);
        if (shop != null)
            shopModel.Id = shop.Id;

        return await Task.FromResult(shopModel);
    }

    private static async Task<ICategoryShopModel> CreateCategoryShopModelAsync(this IShopDataService shopDataService, ICategoryShopImportSettings shopImportSettings)
    {
        var categoryShopModel = await shopDataService.CreateShopModelCoreAsync(shopImportSettings,
        (shopName, shopUrl) => new CategoryShopModel() { ShopName = shopName, ShopUrl = shopUrl, Host = new Uri(shopUrl).Host });

        categoryShopModel.CategorySourceUrl = shopImportSettings.CategorySourceUrl;            

        return categoryShopModel;
    }
}