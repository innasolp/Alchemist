using Alchemist.Common;
using Alchemist.Product.CategoryData;
using Alchemist.Product.Entities;
using Db.Infrastructure;
using Shop.Import.Common;
using Shop.Interfaces;

namespace Shop.Import.Category.Commands;

public class ImportShopCategoryCommandHandler(IShopDataService shopDataService, IShopCachedRepository shopCache)
    : ICommandHandler<ImportShopCategoryCommand, ItemProcessStatus>, ICommandHandler
{
    private readonly IShopDataService _shopDataService = shopDataService;

    private readonly IShopCachedRepository _shopCache = shopCache;

    public async Task<ItemProcessStatus> Handle(ImportShopCategoryCommand request, CancellationToken cancellationToken = default)
    {
        var shop = await _shopCache.TryGetShopAsync(request.Entity.ShopName, request.Entity.ShopUrl, cancellationToken)
            ?? throw new InvalidDataException($"Shop with name {request.Entity.ShopName} or url {request.Entity.ShopUrl} not found.");
       
        var shopCategory = await _shopDataService.GetShopCategoryByShopIdAndItemId(shop.Id, request.Entity.ShopCategory.ItemId, cancellationToken);

        if (shopCategory == null)
        {
            try
            {
                shopCategory = await AddShopCategoryAsync(request.Entity, shop.Id, cancellationToken);
                return ItemProcessStatus.New;
            }
            catch (Exception ex)
            {
                throw new Exception($"Category {request.Entity.ShopCategory.Category} proccessed with error.", ex);
            }
        }
        else        
            return ItemProcessStatus.AlreadyExists;
        
    }

    private async Task<IShopCategory?> AddShopCategoryAsync(CategoryData categoryItem, int shopId, CancellationToken cancellationToken = default)
    {
        var parentCategory = categoryItem.ParentCategory?.ItemId > 0
            ? await _shopDataService.GetShopCategoryByShopIdAndItemId(shopId, categoryItem.ParentCategory.ItemId, cancellationToken)
            : null;

        var shopCategory = new ShopCategory
        {
            ShopId = shopId,
            Category = categoryItem.ShopCategory.Category,
            ItemId = categoryItem.ShopCategory.ItemId,
            Url = categoryItem.ShopCategory.Url,
            ParentId = parentCategory?.Id
        };

        return await _shopDataService.AddShopCategory(shopCategory, cancellationToken);
    }

    Task ICommandHandler.Handle(object command, CancellationToken cancellationToken)
    {
        if (command is ImportShopCategoryCommand importShopCategoryCommand)
            return Handle(importShopCategoryCommand, cancellationToken);

        throw new InvalidOperationException($"Command type is not {typeof(ImportShopCategoryCommand).Name}.");
    }
}