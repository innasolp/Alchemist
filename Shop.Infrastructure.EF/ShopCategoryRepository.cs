using Alchemist.Product.Data;
using Microsoft.EntityFrameworkCore;

namespace Shop.Infrastructure.EF;

public class ShopCategoryRepository(AlchemyContext context) : IShopCategoryRepository
{
    private readonly SemaphoreSlim _addCategoryChildrenSemaphore = new(1, 1);

    private AlchemyContext Context { get; } = context;

    public async Task<List<ShopCategory>> GetAllCategoryChildren(int parentId, CancellationToken cancellationToken = default)
    {
        var children = await Context.ShopCategories
             .Where(c => c.ParentId == parentId).ToListAsync(cancellationToken);

        var result = new List<ShopCategory>();

        foreach (var child in children)
        {
            var categoryChildren = await GetCategoryChildrenTree(child.Id, cancellationToken);
            await AddCategoryChildren(result, categoryChildren, cancellationToken);
        }

        return [.. children.Union(result)];
    }

    private async Task AddCategoryChildren(List<ShopCategory> categories, IEnumerable<ShopCategory> children, CancellationToken token)
    {
        await _addCategoryChildrenSemaphore.WaitAsync(token);
        try
        {
            categories.AddRange(children);
        }
        finally
        {
            _addCategoryChildrenSemaphore.Release();
        }
    }

    private async Task<List<ShopCategory>> GetCategoryChildrenTree(int parentId, CancellationToken token)
    {
        var children = await Context.ShopCategories.Where(c => c.ParentId == parentId).ToListAsync(token);

        var next = new List<ShopCategory>();

        foreach (var child in children)
        {
            var childrenTree = await GetCategoryChildrenTree(child.Id, token);
            next.AddRange(childrenTree);
        }

        return [.. next.Union(children)];
    }

    public Task<List<ShopCategory>> GetShopCategories(int shopId, CancellationToken cancellationToken = default)
    {
        return Context.ShopCategories.Where(su => su.ShopId == shopId).ToListAsync(cancellationToken);        
    }

    public Task<ShopCategory?> GetShopCategoryByShopIdAndItemId(int shopId, int itemId, CancellationToken cancellationToken = default)
    {
        return Context.ShopCategories.FirstOrDefaultAsync(sc => sc.ShopId == shopId && sc.ItemId == itemId, cancellationToken);
    }
}