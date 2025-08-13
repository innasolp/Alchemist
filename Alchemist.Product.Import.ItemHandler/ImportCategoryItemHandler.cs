using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Exceptions;
using Alchemist.Product.DataItem.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.ImportItem.Handler;

internal class ImportCategoryItemHandler(IShopDataService shopDataService, string eventName) : IImportItemHandler
{
    private readonly IShopDataService _shopDataService = shopDataService;

    Type IImportItemHandler.ItemType => typeof(CategoryData);

    public string EventName { get; private set; } = eventName;

    private event AsyncItemHandler<object, ItemProcessStatus>? _itemProcessed;
    public event AsyncItemHandler<object, ItemProcessStatus> ItemProcessed
    {
        add => _itemProcessed += value;
        remove => _itemProcessed -= value;
    }

    public async Task<ItemProcessStatus> HandleItem(object item)
    {
        if (item is not ICategoryData categoryData)
            throw new InvalidDataException($"Item type {item.GetType().Name} is invalid. Expected type must implement {nameof(ICategoryData)}");

        var shopCategory = await _shopDataService.GetShopCategoryByShopIdAndItemId(categoryData.ShopId, categoryData.ShopCategory.ItemId);

        if (shopCategory == null)
        {
            try
            {
                shopCategory = await AddShopCategoryAsync(categoryData);
                await InvokeItemProcessedAsync(categoryData, ItemProcessStatus.New);
                return ItemProcessStatus.New;
            }
            catch (Exception ex)
            {
                throw new WarningException($"Category {categoryData.ShopCategory.Category} proccessed with error.", ex);
            }
        }
        else
        {
            await InvokeItemProcessedAsync(categoryData, ItemProcessStatus.AlreadyExists);
            return ItemProcessStatus.AlreadyExists;
        }
    }

    private Task InvokeItemProcessedAsync(ICategoryData item, ItemProcessStatus itemProcessStatus)
    {
        return _itemProcessed?.Invoke(this, item, itemProcessStatus) ?? Task.FromResult(false);
    }

    private async Task<IShopCategory?> AddShopCategoryAsync(ICategoryData categoryItem)
    {
        var parentCategory = categoryItem.ParentCategory?.ItemId > 0 
            ? await _shopDataService.GetShopCategoryByShopIdAndItemId(categoryItem.ShopId, categoryItem.ParentCategory.ItemId) 
            : null;
        var shopCatergory = new ShopCategory { 
            ShopId = categoryItem.ShopId,
            Category = categoryItem.ShopCategory.Category,
            ItemId = categoryItem.ShopCategory.ItemId,
            ParentId = parentCategory?.Id };
        return await _shopDataService.AddShopCategory(shopCatergory);
    }
}
