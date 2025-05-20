using Alchemist.Common;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Product.Interfaces;
using System.ComponentModel;
using Alchemist.DataService.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Import.Interfaces;

namespace Alchemist.Import.Categories.Data;

internal class CategoriesDataHandler(IShopDataService shopDataService) : ICategoryItemHandler
{
    private readonly IShopDataService _shopDataService = shopDataService;

    public event AsyncItemHandler<ICategory> ItemProcessed;

    public async Task<ItemProcessStatus> HandleItem(ICategory category, IShopModel shopModel)
    {
        //todo
        var shop = shopModel as IShop;
        var shopId = shop.Id;

        var shopCategory = await _shopDataService.GetShopCategoryByShopIdAndItemId(shopId, category.Id);

        if (shopCategory != null)
        {
            await InvokeItemProcessedAsync(category, shopModel, ItemProcessStatus.AlreadyExists);
            return ItemProcessStatus.AlreadyExists;            
        }
        else
        {
            try
            {
                shopCategory = await AddShopCategoryAsync(shopId, category);
                await InvokeItemProcessedAsync(category, shopModel, ItemProcessStatus.New);
                return ItemProcessStatus.New;
            }
            catch (Exception ex)
            {
                await InvokeItemProcessedAsync(category, shopModel, ItemProcessStatus.Warning);
                throw new WarningException($"Category {category.Url} proccessed with error.", ex);
            }
        }
    }

    private Task InvokeItemProcessedAsync(ICategory category, IShopModel shopModel, ItemProcessStatus itemProcessStatus)
    {
        return ItemProcessed?.Invoke(this, category, shopModel, itemProcessStatus) ?? Task.FromResult(false);
    }

    private async Task<IShopCategory?> AddShopCategoryAsync(int shopId, ICategory category)
    {
        var parentCategory = category.ParentId != null ? await _shopDataService.GetShopCategoryByShopIdAndItemId(shopId, (int)category.ParentId) : null;
        var shopCatergory = new ShopCategory { ShopId = shopId, Category = category.Url, ItemId = category.Id, ParentId = parentCategory?.Id };
        return await _shopDataService.AddShopCategory(shopCatergory);
    }

    async Task<ItemProcessStatus> IItemHandler.HandleItem(object item, IShopModel shopModel)
    {
        if (item is not ICategory category)
            return await Task.FromResult(ItemProcessStatus.Error);

        return await HandleItem(category, shopModel);
    }
}
