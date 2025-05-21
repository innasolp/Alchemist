using Alchemist.DataService.Interfaces;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.Model;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Background;

internal static class ShopModelHelpers
{
    internal static async Task<IProductShopModel> CreateProductShopModelAsync(this IShopDataService shopDataService, IProductShopImportSettings shopImportSettings)
    {
        var productShopModel = await shopDataService.CreateShopModelAsync<ProductShopModel>(shopImportSettings);

        productShopModel.ProductUrl = shopImportSettings.ProductUrl;
        productShopModel.CategoryUrl = shopImportSettings.CategoryUrl;        

        if (productShopModel.Id != 0)
        {
            var categoriesService = shopImportSettings.Services.OfType<ImportServiceSettings>().FirstOrDefault(s=>s.Value?.ToString().Contains("Categories") == true);
            var categories = categoriesService?.Value["Categories"]?.AsArray().Select(v=>v.ToString());

            var shopCategories = await shopDataService.GetShopCategories(productShopModel.Id);
            shopCategories?.ForEach(c => productShopModel.Categories.Add(new ProductShopCategoryModel { Category = c.Category, ItemId = c.ItemId }));
        }

        return productShopModel;
    }

    private static async Task<T> CreateShopModelAsync<T>(this IShopDataService shopDataService, IShopImportSettings shopImportSettings)
        where T: class, IShop, IShopModel, new()
    {
        var shop = (shopImportSettings.Id != 0
                ? await shopDataService.GetShop(shopImportSettings.ShopId)
                : await shopDataService.GetShopByName(shopImportSettings.ShopName ?? shopImportSettings.Name)
                ?? await shopDataService.GetShopByUrl(shopImportSettings.ShopUrl ?? shopImportSettings.Url))
                ?? await shopDataService.CreateShop(new Shop { Name = shopImportSettings.ShopName, Url = shopImportSettings.ShopUrl });

        return shop != null
                ? await Task.FromResult(new T { ShopName = shop.Name, ShopUrl = shop.Url, Id = shop.Id })
                : await Task.FromResult(new T
                {
                    ShopName = shopImportSettings.ShopName ?? shopImportSettings.Name,
                    ShopUrl = shopImportSettings.ShopUrl ?? shopImportSettings.Url
                });
    }

    internal static async Task<ICategoryShopModel> CreateCategoryShopModelAsync(this IShopDataService shopDataService, ICategoryShopImportSettings shopImportSettings)
    {
        var categoryShopModel = await shopDataService.CreateShopModelAsync<CategoryShopModel>(shopImportSettings);

        categoryShopModel.CategorySourceUrl = shopImportSettings.CategorySourceUrl;            

        return categoryShopModel;
    }
}