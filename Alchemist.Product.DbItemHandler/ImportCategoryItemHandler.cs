using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Exceptions;
using Alchemist.Product.DataItem.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.DbItemHandler;

internal class ImportCategoryItemHandler(IShopDataService shopDataService, string eventName, IShopCachedRepository shopCache) : IImportItemHandler
{
    private class ImportCategoryProcessEventArgs(ICategoryData item, ItemProcessStatus processStatus, CancellationToken cancellationToken)
        : ItemProcessEventArgs<object, ItemProcessStatus>(item, processStatus, cancellationToken)
    { }

    private readonly IShopDataService _shopDataService = shopDataService;

    private readonly IShopCachedRepository _shopCache = shopCache;

    Type IImportItemHandler.ItemType => typeof(CategoryData);

    public string EventName { get; private set; } = eventName;

    private event AsyncEventHandler<ItemProcessEventArgs<object, ItemProcessStatus>>? _itemProcessed;
    public event AsyncEventHandler<ItemProcessEventArgs<object, ItemProcessStatus>> ItemProcessed
    {
        add => _itemProcessed += value;
        remove => _itemProcessed -= value;
    }

    public async Task<ItemProcessStatus> HandleItem(object item, CancellationToken cancellationToken = default)
    {
        if (item is not ICategoryData categoryData)
            throw new InvalidDataException($"Item type {item.GetType().Name} is invalid. Expected type must implement {nameof(ICategoryData)}");
        
        var shop = await _shopCache.TryGetShopAsync(categoryData.ShopName, categoryData.ShopUrl, cancellationToken)
            ?? throw new InvalidDataException($"Shop with name {categoryData.ShopName} or url {categoryData.ShopUrl} not found.");
        var shopCategory = await _shopDataService.GetShopCategoryByShopIdAndItemId(shop.Id, categoryData.ShopCategory.ItemId, cancellationToken);

        if (shopCategory == null)
        {
            try
            {
                shopCategory = await AddShopCategoryAsync(categoryData, shop.Id, cancellationToken);
                await InvokeItemProcessedAsync(categoryData, ItemProcessStatus.New, cancellationToken);
                return ItemProcessStatus.New;
            }
            catch (Exception ex)
            {
                throw new WarningException($"Category {categoryData.ShopCategory.Category} proccessed with error.", ex);
            }
        }
        else
        {
            await InvokeItemProcessedAsync(categoryData, ItemProcessStatus.AlreadyExists, cancellationToken);
            return ItemProcessStatus.AlreadyExists;
        }
    }

    

    private Task InvokeItemProcessedAsync(ICategoryData item, ItemProcessStatus itemProcessStatus, CancellationToken cancellationToken = default)
    {
        return _itemProcessed?.Invoke(this, new ImportCategoryProcessEventArgs(item, itemProcessStatus, cancellationToken)) ?? Task.FromResult(false);
    }

    private async Task<IShopCategory?> AddShopCategoryAsync(ICategoryData categoryItem, int shopId, CancellationToken cancellationToken = default)
    {
        var parentCategory = categoryItem.ParentCategory?.ItemId > 0 
            ? await _shopDataService.GetShopCategoryByShopIdAndItemId(shopId, categoryItem.ParentCategory.ItemId, cancellationToken) 
            : null;
        var shopCatergory = new ShopCategory { 
            ShopId = shopId,
            Category = categoryItem.ShopCategory.Category,
            ItemId = categoryItem.ShopCategory.ItemId,
            ParentId = parentCategory?.Id };
        return await _shopDataService.AddShopCategory(shopCatergory, cancellationToken);
    }
}