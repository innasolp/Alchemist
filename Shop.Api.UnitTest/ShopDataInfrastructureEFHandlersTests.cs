using Alchemist.Product.Data;
using Db.Infrastructure.Requests;
using Microsoft.EntityFrameworkCore;


namespace Shop.Data.Infrastructure.EF.Test;

public class ShopDataInfrastructureEFHandlersTests
{
    private sealed class TestAlchemyContext(string dbName) : AlchemyContext
    {
        private readonly string _dbName = dbName;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(_dbName);
            base.OnConfiguring(optionsBuilder);
        }
    }

    private static AlchemyContext CreateInMemoryContext(string dbName) => new TestAlchemyContext(dbName);

    [Fact]
    public async Task FindShopByName_ReturnsShop_WhenExists()
    {
        var dbName = nameof(FindShopByName_ReturnsShop_WhenExists);
        await using var ctx = CreateInMemoryContext(dbName);
        ctx.Shops.Add(new Alchemist.Product.Data.Shop { Name = "MyShop", Url = "u" });
        await ctx.SaveChangesAsync();

        var handler = new FindShopByNameRequestHandler(ctx);
        var shop = await handler.Handle(new FindByNameRequest<Alchemist.Product.Data.Shop>("MyShop"), CancellationToken.None);

        Assert.NotNull(shop);
        Assert.Equal("MyShop", shop!.Name);
    }

    [Fact]
    public async Task FindShopByName_ReturnsNull_WhenNotExists()
    {
        var dbName = nameof(FindShopByName_ReturnsNull_WhenNotExists);
        await using var ctx = CreateInMemoryContext(dbName);

        var handler = new FindShopByNameRequestHandler(ctx);
        var shop = await handler.Handle(new FindByNameRequest<Alchemist.Product.Data.Shop>("DoesNotExist"), CancellationToken.None);

        Assert.Null(shop);
    }

    [Fact]
    public async Task GetShopByUrl_ReturnsShop_WhenExists()
    {
        var dbName = nameof(GetShopByUrl_ReturnsShop_WhenExists);
        await using var ctx = CreateInMemoryContext(dbName);
        ctx.Shops.Add(new Alchemist.Product.Data.Shop { Name = "myshop", Url = "https://shop" });
        await ctx.SaveChangesAsync();

        var handler = new GetShopByUrlRequestHandler(ctx);
        var shop = await handler.Handle(new GetShopByUrlRequest("https://shop"), CancellationToken.None);

        Assert.NotNull(shop);
        Assert.Equal("https://shop", shop.Url);
    }

    [Fact]
    public async Task GetShopByUrl_ReturnsNull_WhenNotExists()
    {
        var dbName = nameof(GetShopByUrl_ReturnsNull_WhenNotExists);
        await using var ctx = CreateInMemoryContext(dbName);

        var handler = new GetShopByUrlRequestHandler(ctx);
        var shop = await handler.Handle(new GetShopByUrlRequest("https://nope"), CancellationToken.None);

        Assert.Null(shop);
    }

    [Fact]
    public async Task GetShopCategories_ReturnsCategories_ForShop()
    {
        var dbName = nameof(GetShopCategories_ReturnsCategories_ForShop);
        await using var ctx = CreateInMemoryContext(dbName);
        ctx.ShopCategories.AddRange(
            new ShopCategory { ShopId = 1, ItemId = 10, Category = "C1" },
            new ShopCategory { ShopId = 1, ItemId = 11, Category = "C2" },
            new ShopCategory { ShopId = 2, ItemId = 12, Category = "Other" }
        );
        await ctx.SaveChangesAsync();

        var handler = new GetShopCategoriesRequestHandler(ctx);
        var categories = await handler.Handle(new GetShopCategoriesRequest(1), CancellationToken.None);

        Assert.Equal(2, categories.Count);
        Assert.All(categories, c => Assert.Equal(1, c.ShopId));
    }

    [Fact]
    public async Task GetShopCategories_ReturnsEmpty_WhenNone()
    {
        var dbName = nameof(GetShopCategories_ReturnsEmpty_WhenNone);
        await using var ctx = CreateInMemoryContext(dbName);

        var handler = new GetShopCategoriesRequestHandler(ctx);
        var categories = await handler.Handle(new GetShopCategoriesRequest(999), CancellationToken.None);

        Assert.Empty(categories);
    }

    [Fact]
    public async Task GetShopCategoryByShopIdAndItemId_ReturnsCategory_WhenExists()
    {
        var dbName = nameof(GetShopCategoryByShopIdAndItemId_ReturnsCategory_WhenExists);
        await using var ctx = CreateInMemoryContext(dbName);
        ctx.ShopCategories.Add(new ShopCategory { ShopId = 5, ItemId = 55, Category = "Cat" });
        await ctx.SaveChangesAsync();

        var handler = new GetShopCategoryByShopIdAndItemIdRequestHandler(ctx);
        var cat = await handler.Handle(new GetShopCategoryByShopIdAndItemIdRequest(5, 55), CancellationToken.None);

        Assert.NotNull(cat);
        Assert.Equal(5, cat.ShopId);
        Assert.Equal(55, cat.ItemId);
    }

    [Fact]
    public async Task GetShopCategoryByShopIdAndItemId_ReturnsNull_WhenNotExists()
    {
        var dbName = nameof(GetShopCategoryByShopIdAndItemId_ReturnsNull_WhenNotExists);
        await using var ctx = CreateInMemoryContext(dbName);

        var handler = new GetShopCategoryByShopIdAndItemIdRequestHandler(ctx);
        var cat = await handler.Handle(new GetShopCategoryByShopIdAndItemIdRequest(123, 456), CancellationToken.None);

        Assert.Null(cat);
    }

    [Fact]
    public async Task CheckCategoryForAncestorItem_ReturnsTrue_WhenAncestorInPath()
    {
        var dbName = nameof(CheckCategoryForAncestorItem_ReturnsTrue_WhenAncestorInPath);
        await using var ctx = CreateInMemoryContext(dbName);

        var ancestor = new ShopCategory { Id = 2, ItemId = 100, Category = "Anc" };
        var child = new ShopCategory { Id = 5, ItemId = 200, Path = "/1/2/", Category = "Child" };

        ctx.ShopCategories.AddRange(ancestor, child);
        await ctx.SaveChangesAsync();

        var handler = new CheckCategoryForAncestorItemRequestHandler(ctx);
        var result = await handler.Handle(new CheckCategoryForAncestorItemRequest(child.Id, ancestor.ItemId), CancellationToken.None);

        Assert.True(result.HasValue && result.Value);
    }

    [Fact]
    public async Task CheckCategoryForAncestorItem_ReturnsNull_WhenCategoryNotFound()
    {
        var dbName = nameof(CheckCategoryForAncestorItem_ReturnsNull_WhenCategoryNotFound);
        await using var ctx = CreateInMemoryContext(dbName);

        var handler = new CheckCategoryForAncestorItemRequestHandler(ctx);
        var result = await handler.Handle(new CheckCategoryForAncestorItemRequest(999, 100), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllCategoryChildren_ReturnsDescendants()
    {
        var dbName = nameof(GetAllCategoryChildren_ReturnsDescendants);
        await using var ctx = CreateInMemoryContext(dbName);

        var root = new ShopCategory { Id = 1, ItemId = 1, Category = "Root", ParentId = 0, Path = "/1/" };
        var child = new ShopCategory { Id = 2, ItemId = 2, Category = "Child", ParentId = 1, Path = "/1/2/" };
        var grand = new ShopCategory { Id = 3, ItemId = 3, Category = "Grand", ParentId = 2, Path = "/1/2/3/" };

        ctx.ShopCategories.AddRange(root, child, grand);
        await ctx.SaveChangesAsync();

        var handler = new GetAllCategoryChildrenRequestHandler(ctx);
        var result = await handler.Handle(new GetAllCategoryChildrenRequest(1), CancellationToken.None);

        var ids = result.Select(r => r.Id).ToHashSet();
        Assert.Contains(2, ids);
        Assert.Contains(3, ids);
    }

    [Fact]
    public async Task GetAllCategoryChildren_ReturnsEmpty_WhenNoChildren()
    {
        var dbName = nameof(GetAllCategoryChildren_ReturnsEmpty_WhenNoChildren);
        await using var ctx = CreateInMemoryContext(dbName);

        var handler = new GetAllCategoryChildrenRequestHandler(ctx);
        var result = await handler.Handle(new GetAllCategoryChildrenRequest(999), CancellationToken.None);

        Assert.Empty(result);
    }
}