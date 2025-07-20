using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Exceptions;
using Alchemist.Product.Entities;
using Alchemist.Product.ImportItem.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.ImportItem.Handler;

internal class ImportCategoryItemHandler(IShopDataService shopDataService) : IItemHandler<IImportCategoryItem>
{
    private readonly IShopDataService _shopDataService = shopDataService;

    public async Task<ItemProcessStatus> HandleItem(IImportCategoryItem categoryItem)
    {
        var shopCategory = await _shopDataService.GetShopCategoryByShopIdAndItemId(categoryItem.ShopId, categoryItem.ShopCategory.ItemId);

        if (shopCategory == null)
        {
            try
            {
                shopCategory = await AddShopCategoryAsync(categoryItem);
                return ItemProcessStatus.New;
            }
            catch (Exception ex)
            {
                throw new WarningException($"Category {categoryItem.ShopCategory.Category} proccessed with error.", ex);
            }
        }
        else        
            return ItemProcessStatus.AlreadyExists;        
    }

    private async Task<IShopCategory?> AddShopCategoryAsync(IImportCategoryItem categoryItem)
    {
        var parentCategory = categoryItem.ParentCategory.ItemId > 0 ? await _shopDataService.GetShopCategoryByShopIdAndItemId(categoryItem.ShopId, categoryItem.ParentCategory.ItemId) : null;
        var shopCatergory = new ShopCategory { ShopId = categoryItem.ShopId, Category = categoryItem.ShopCategory.Category, ItemId = categoryItem.ShopCategory.ItemId, ParentId = parentCategory?.Id };
        return await _shopDataService.AddShopCategory(shopCatergory);
    }

    async Task<ItemProcessStatus> IItemHandler.HandleItem(object item)
    {
        if (item is not IImportCategoryItem categoryItem)
            throw new InvalidOperationException($"Invalid item type {item.GetType().Name}. Must be {nameof(IImportCategoryItem)}.");

        return await HandleItem(categoryItem);
    }
}
