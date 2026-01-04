using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.Category;
using Import.Settings.Interfaces;
using Alchemist.Import.Settings.Product;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Alchemist.Import.Settings;

namespace Alchemist.Product.Import.Background.Models;

internal static class ShopModelExtensions
{
    internal static async Task<IImportSource> GetShopModelAsync(this IShopDataService shopDataService,
        IShopImportSettings shopImportSettings, 
        ShopSettingType shopSettingType,
        CancellationToken cancellationToken = default)
    {
        return shopSettingType == ShopSettingType.Product
            ? await shopDataService.GetProductShopModelAsync(shopImportSettings as IProductShopImportSettings, cancellationToken)
            : await shopDataService.GetCategoryShopModelAsync(shopImportSettings as ICategoryShopImportSettings, cancellationToken: cancellationToken);
    }

    internal static async Task<IImportSource> GetShopModelAsync(this IShopDataService shopDataService,
        IShopImportSettings shopImportSettings,
        CancellationToken cancellationToken = default)
    {
        var shopSettingsType = shopImportSettings is IShopSettings shopSettings ? shopSettings.Type
             : shopImportSettings is IProductShopImportSettings ? ShopSettingType.Product :
              shopImportSettings is ICategoryShopImportSettings ? ShopSettingType.Category
              : throw new InvalidOperationException($"{shopImportSettings.GetType().Name} unavailable shop settings type");

        return shopSettingsType == ShopSettingType.Product
            ? await shopDataService.GetProductShopModelAsync(shopImportSettings as IProductShopImportSettings, cancellationToken: cancellationToken)
            : await shopDataService.GetCategoryShopModelAsync(shopImportSettings as ICategoryShopImportSettings, cancellationToken: cancellationToken);        

    }

    private static async Task<IProductShopModel> GetProductShopModelAsync(this IShopDataService shopDataService,
        IProductShopImportSettings shopImportSettings, 
        CancellationToken cancellationToken = default)
    {
        var productShopModel = await shopDataService.GetShopModelCoreAsync<ProductShopModel>(shopImportSettings, cancellationToken);

        productShopModel.ProductUrl = shopImportSettings.ProductUrlFormat;
        productShopModel.CategoryUrl = shopImportSettings.CategoryUrlFormat;

        if (productShopModel.Id != 0)
        {
            if (shopImportSettings.RootCategories?.Count() > 0)
            {
                var shopCategories = new List<IShopCategory>();

                foreach (var c in shopImportSettings.RootCategories)
                {
                    var shopCategory = await shopDataService.GetShopCategoryByShopIdAndItemId(productShopModel.Id, c.Item, cancellationToken);
                    if (shopCategory != null)
                    {
                        var children = await shopDataService.GetAllCategoryChildren(shopCategory.Id, cancellationToken);
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
                var allShopCategories = await shopDataService.GetShopCategories(productShopModel.Id, cancellationToken);
                var categories2 = new List<IShopCategory>(allShopCategories);
                var lastShopCategories = allShopCategories.Where(c => !categories2.Any(c2 => c2.ParentId == c.Id)).ToList();
                lastShopCategories.ForEach(c => productShopModel.Categories.Add(new ProductShopCategoryModel { Category = c.Category, ItemId = c.ItemId }));
            }
        }
        else
            shopImportSettings.RootCategories?.ToList().ForEach(c => productShopModel.Categories.Add(new ProductShopCategoryModel { Category = c.Url, ItemId = c.Item }));

        return productShopModel;
    }

    private static async Task<T> GetShopModelCoreAsync<T>(this IShopDataService shopDataService, 
        IShopImportSettings shopImportSettings,
        CancellationToken cancellationToken = default)
        where T : ShopModel, new()
    {
        var shop = (shopImportSettings is ShopSettings shopSettings
                ? await shopDataService.GetShop(shopSettings.ShopId, cancellationToken)
                : await shopDataService.GetShopByName(shopImportSettings.ShopName, cancellationToken)
                ?? await shopDataService.GetShopByUrl(shopImportSettings.ShopUrl, cancellationToken))
                ?? await shopDataService.CreateShop(new Shop { Name = shopImportSettings.ShopName, Url = shopImportSettings.ShopUrl }, cancellationToken);

        var shopModel = CreateShopModelCore<T>(shop, shopImportSettings);

        return await Task.FromResult(shopModel);
    }

    private static async Task<ICategoryShopModel> GetCategoryShopModelAsync(this IShopDataService shopDataService,
        ICategoryShopImportSettings shopImportSettings,
        CancellationToken cancellationToken = default)
    {
        var categoryShopModel = await shopDataService.GetShopModelCoreAsync<CategoryShopModel>(shopImportSettings, cancellationToken);

        categoryShopModel.CategorySourceUrl = shopImportSettings.CategorySourceUrl;

        return categoryShopModel;
    }    

    private static T CreateShopModelCore<T>(IShop shop, IShopImportSettings shopImportSettings)
        where T : ShopModel, new()
    {
        return new T
        {
            Name = shop?.Name ?? shopImportSettings.ShopName,
            Url = shop?.Url ?? shopImportSettings.ShopUrl,
            Id = shop.Id
        };
    }
}