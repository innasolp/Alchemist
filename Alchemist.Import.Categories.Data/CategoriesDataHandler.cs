using Alchemist.Common;
using Alchemist.Product.Entities;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Product.Interfaces;
using System.ComponentModel;
using Alchemist.Import.Shop.Interfaces;
using Alchemist.Product.DataService.Interfaces;

namespace Alchemist.Import.Categories.Data;

internal class CategoriesDataHandler(IShopDataService shopDataService) : ICategoryDataHandler
{
    private readonly IShopDataService _shopDataService = shopDataService;

    public async Task<ItemProcessStatus> HandleItem(ICategory category, int shopId)
    {
        var shopCategory = await _shopDataService.GetShopCategoryByShopIdAndItemId(shopId, category.Id);

        if (shopCategory != null)
            return ItemProcessStatus.AlreadyExists;
        else
        {
            try
            {
                shopCategory = await AddShopCategoryAsync(shopId, category);
                return ItemProcessStatus.New;
            }
            catch (Exception ex)
            {
                throw new WarningException($"Category {category.Url} proccessed with error.", ex);
            }
        }
    }

    private async Task<IShopCategory?> AddShopCategoryAsync(int shopId, ICategory category)
    {
        var parentCategory = category.ParentId != null ? await _shopDataService.GetShopCategoryByShopIdAndItemId(shopId, (int)category.ParentId) : null;
        var shopCatergory = new ShopCategory { ShopId = shopId, Category = category.Url, ItemId = category.Id, ParentId = parentCategory?.Id };
        return await _shopDataService.AddShopCategory(shopCatergory);
    }

    async Task<ItemProcessStatus> IItemHandler.HandleItem(object item, int shopId)
    {
        if (item is not ICategory category)
            return await Task.FromResult(ItemProcessStatus.Error);

        return await HandleItem(category, shopId);
    }
}
