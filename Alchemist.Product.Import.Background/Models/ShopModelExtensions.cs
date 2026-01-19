using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings;
using Alchemist.Import.Settings.Category;
using Alchemist.Import.Settings.Product;
using Import.Settings.Interfaces;
using Shop.Interfaces;
using ShopSettings.Interfaces;

namespace Alchemist.Product.Import.Background.Models;

internal static class ShopModelExtensions
{
    internal static IProductShopCategory ToProductShopCategoryModel(this IShopCategory c)
    {
        return new ProductShopCategoryModel { Category = c.Category, ItemId = c.ItemId, Path = c.Url };
    }

    internal static IProductShopCategory ToProductShopCategoryModel(this IProductShopCategory c)
    {
        return new ProductShopCategoryModel { Category = c.Category, ItemId = c.ItemId, Path = c.Path };
    }

    internal static async Task<IImportSource> GetShopModelAsync(this IShopDataService shopDataService,
        IShopImportSettings shopImportSettings, 
        ShopSettingType shopSettingType,
        CancellationToken cancellationToken = default)
    {
        return shopSettingType == ShopSettingType.Product
            ? await shopDataService.GetProductShopModelAsync(shopImportSettings as IProductShopImportSettings, cancellationToken)
            : await shopDataService.GetCategoryShopModelAsync(shopImportSettings as ICategoryShopImportSettings, cancellationToken: cancellationToken);
    }

    internal static async Task<IShopModel> GetShopModelAsync(this IShopDataService shopDataService,
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
                        if (children.Count != 0)
                        {
                            var children2 = new List<IShopCategory>(children);
                            shopCategories.AddRange(children.Where(child => !children2.Any(c2 => c2.ParentId == child.Id)));
                        }
                        else
                            shopCategories.Add(shopCategory);
                    }
                }

                if (shopCategories.Count > 0)
                    shopCategories?.ForEach(c => productShopModel.Categories.Add(c.ToProductShopCategoryModel()));
                //todo
                else
                   shopImportSettings.RootCategories.ToList().ForEach(c => productShopModel.RootCategories.Add(new ProductShopCategoryModel { Category = c.Url, ItemId = c.Item, Path = c.Url }));
            }
            else
            {
                var allShopCategories = await shopDataService.GetShopCategories(productShopModel.Id, cancellationToken);
                var categories2 = new List<IShopCategory>(allShopCategories);
                var lastShopCategories = allShopCategories.Where(c => !categories2.Any(c2 => c2.ParentId == c.Id)).ToList();
                lastShopCategories.ForEach(c => productShopModel.Categories.Add(c.ToProductShopCategoryModel()));
            }
        }
        else
            shopImportSettings.RootCategories?.ToList().ForEach(c => productShopModel.Categories.Add(new ProductShopCategoryModel { Category = c.Url, ItemId = c.Item, Path = c.Url }));

        return productShopModel;
    }

    private static async Task<T> GetShopModelCoreAsync<T>(this IShopDataService shopDataService, 
        IShopImportSettings shopImportSettings,
        CancellationToken cancellationToken = default)
        where T : ShopModel, new()
    {
        var shop = (shopImportSettings is IShopSettings shopSettings
                ? await shopDataService.GetShop(shopSettings.ShopId, cancellationToken)
                : await shopDataService.GetShopByName(shopImportSettings.ShopName, cancellationToken)
                ?? await shopDataService.GetShopByUrl(shopImportSettings.ShopUrl, cancellationToken))
                ?? await shopDataService.CreateShop(new ShopModel { Name = shopImportSettings.ShopName, Url = shopImportSettings.ShopUrl }, cancellationToken);

        var shopModel = GetShopModelCore<T>(shop, shopImportSettings);

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

    private static T GetShopModelCore<T>(IShop shop, IShopImportSettings shopImportSettings)
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