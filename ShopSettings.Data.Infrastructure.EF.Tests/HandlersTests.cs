using Alchemist.Product.Data;
using Db.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Collections.Concurrent;
using ShopSettings.Data.infrastructure.EF;

namespace ShopSettings.Data.Infrastructure.EF.Tests;

public class HandlersTests
{
    private sealed class TestAlchemyContext : AlchemyContext
    {
        private static readonly AsyncLocal<string?> _currentDbName = new();
        private readonly string _dbName;

        private static readonly ConcurrentDictionary<string, SqliteConnection> _connections = new();

        public TestAlchemyContext()
        {
            _dbName = _currentDbName.Value ?? System.Guid.NewGuid().ToString();
        }

        public static void SetCurrentDbName(string dbName) => _currentDbName.Value = dbName;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connection = _connections.GetOrAdd(_dbName, _ =>
            {
                var conn = new SqliteConnection("DataSource=:memory:");
                conn.Open();
                return conn;
            });

            optionsBuilder.UseSqlite(connection);
            base.OnConfiguring(optionsBuilder);
        }
    }

    private static AlchemyContext CreateContext(string dbName)
    {
        TestAlchemyContext.SetCurrentDbName(dbName);
        var ctx = new TestAlchemyContext();
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public async Task GetShopSettingsByShopIdHandler_ReturnsShop_WhenExists()
    {
        var dbName = nameof(GetShopSettingsByShopIdHandler_ReturnsShop_WhenExists);
        await using var ctx = CreateContext(dbName);

        var shopSettings = new Alchemist.Product.Data.ShopSettings { ShopId = 5, Name = "n", IsActual = true, JsonValue = "{}", Type = ShopSettingType.Category };
        ctx.ShopSettings.Add(shopSettings);
        await ctx.SaveChangesAsync();

        var handler = new GetShopSettingsByShopIdRequestHandler(ctx);

        var result = await handler.Handle(new GetShopSettingsByShopIdRequest(5, ShopSettingType.Category), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(5, result!.ShopId);
    }

    [Fact]
    public async Task GetChildSettingsHandler_ReturnsChildren()
    {
        var dbName = nameof(GetChildSettingsHandler_ReturnsChildren);
        await using var ctx = CreateContext(dbName);

        var parent = new Alchemist.Product.Data.ShopSettings { ShopId = 1, Name = "p", JsonValue = "{}", Type = ShopSettingType.Category };
        await ctx.AddAsync(parent);
        await ctx.SaveChangesAsync();

        var child1 = new Alchemist.Product.Data.ShopSettings { ParentSettingsId = parent.Id, ShopId = 1, Name = "c1", JsonValue = "{}", Type = ShopSettingType.Category };
        var child2 = new Alchemist.Product.Data.ShopSettings { ParentSettingsId = parent.Id, ShopId = 1, Name = "c2", JsonValue = "{}", Type = ShopSettingType.Category };
        await ctx.AddRangeAsync(child1, child2);
        await ctx.SaveChangesAsync();

        var handler = new GetChildSettingsRequestHandler(ctx);

        var list = await handler.Handle(new GetChildSettingsRequest(1), CancellationToken.None);

        Assert.Equal(2, list.Count);
        Assert.All(list, s => Assert.Equal(1, s.ParentSettingsId));
    }

    [Fact]
    public async Task GetAllParentShopSettingsHandler_ReturnsParents()
    {
        var dbName = nameof(GetAllParentShopSettingsHandler_ReturnsParents);
        await using var ctx = CreateContext(dbName);

        var p1 = new Alchemist.Product.Data.ShopSettings { ShopId = 1, Name = "p1", JsonValue = "{}", Type = ShopSettingType.Category };
        var p2 = new Alchemist.Product.Data.ShopSettings { ShopId = 2, Name = "p2", JsonValue = "{}", Type = ShopSettingType.Category };
        var child = new Alchemist.Product.Data.ShopSettings { ParentSettingsId = 1, ShopId = 1, Name = "c", JsonValue = "{}", Type = ShopSettingType.Category };
        await ctx.AddRangeAsync(p1, p2, child);
        await ctx.SaveChangesAsync();

        var handler = new GetAllParentShopSettingsRequestHandler(ctx);

        var list = await handler.Handle(new GetAllParentShopSettingsRequest(), CancellationToken.None);

        Assert.True(list.Count >= 2);
        Assert.All(list, s => Assert.Null(s.ParentSettingsId));
    }

    [Fact]
    public async Task SaveShopSettingsCommandHandler_SavesNewEntity()
    {
        var dbName = nameof(SaveShopSettingsCommandHandler_SavesNewEntity);
        await using var ctx = CreateContext(dbName);

        var uow = new EFUnitOfWork<AlchemyContext>(ctx);
        var handler = new SaveShopSettingsCommandHandler(ctx, uow);

        var entity = new Alchemist.Product.Data.ShopSettings { ShopId = 10, Name = "new", JsonValue = "{}", Type = ShopSettingType.Category };
        await handler.Handle(new SaveShopSettingsCommand(entity), CancellationToken.None);

        var saved = await ctx.ShopSettings.FirstOrDefaultAsync(s => s.ShopId == 10);
        Assert.NotNull(saved);
        Assert.Equal("new", saved!.Name);
    }

    [Fact]
    public async Task SaveShopSettingsWithChildrenCommandHandler_SavesParentAndChildren()
    {
        var dbName = nameof(SaveShopSettingsWithChildrenCommandHandler_SavesParentAndChildren);
        await using var ctx = CreateContext(dbName);

        var uow = new EFUnitOfWork<AlchemyContext>(ctx);
        var handler = new SaveShopSettingsWithChildrenCommandHandler(ctx, uow);

        var parent = new Alchemist.Product.Data.ShopSettings { ShopId = 20, Name = "parent", JsonValue = "{}", Type = ShopSettingType.Category };
        var s1 = new Alchemist.Product.Data.ShopSettings { ShopId = 20, Name = "s1", JsonValue = "{}", Type = ShopSettingType.Service };
        var s2 = new Alchemist.Product.Data.ShopSettings { ShopId = 20, Name = "s2", JsonValue = "{}", Type = ShopSettingType.Service };

        await handler.Handle(new SaveShopSettingsWithChildrenCommand(parent, new[] { s1, s2 }), CancellationToken.None);

        var savedParent = await ctx.ShopSettings.FirstOrDefaultAsync(s => s.ShopId == 20 && s.Type != ShopSettingType.Service);
        Assert.NotNull(savedParent);

        var services = await ctx.ShopSettings.Where(s => s.ParentSettingsId == savedParent.Id).ToListAsync();
        Assert.Equal(2, services.Count);
    }
    
    [Fact]
    public async Task GetShopSettingsByShopIdHandler_ReturnsNull_WhenNotExists()
    {
        var dbName = nameof(GetShopSettingsByShopIdHandler_ReturnsNull_WhenNotExists);
        await using var ctx = CreateContext(dbName);

        var shopSettings = new Alchemist.Product.Data.ShopSettings { ShopId = 5, Name = "n", IsActual = true, JsonValue = "{}", Type = ShopSettingType.Category };
        ctx.ShopSettings.Add(shopSettings);
        await ctx.SaveChangesAsync();

        var handler = new GetShopSettingsByShopIdRequestHandler(ctx);

        var result = await handler.Handle(new GetShopSettingsByShopIdRequest(1, ShopSettingType.Category), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetChildSettingsHandler_ReturnsEmpty_WhenNoChildrenOfParent()
    {
        var dbName = nameof(GetChildSettingsHandler_ReturnsEmpty_WhenNoChildrenOfParent);
        await using var ctx = CreateContext(dbName);

        var parent = new Alchemist.Product.Data.ShopSettings { ShopId = 1, Name = "p", JsonValue = "{}", Type = ShopSettingType.Category };
        await ctx.AddAsync(parent);
        await ctx.SaveChangesAsync();

        var child1 = new Alchemist.Product.Data.ShopSettings { ParentSettingsId = 4, ShopId = 1, Name = "c1", JsonValue = "{}", Type = ShopSettingType.Category };
        var child2 = new Alchemist.Product.Data.ShopSettings { ParentSettingsId = 5, ShopId = 1, Name = "c2", JsonValue = "{}", Type = ShopSettingType.Category };
        await ctx.AddRangeAsync(child1, child2);
        await ctx.SaveChangesAsync();

        var handler = new GetChildSettingsRequestHandler(ctx);

        var list = await handler.Handle(new GetChildSettingsRequest(1), CancellationToken.None);

        Assert.Empty(list);
    }

    [Fact]
    public async Task GetAllParentShopSettingsHandler_ReturnsEmpty_WhenAllShopSettingsAreChildren()
    {
        var dbName = nameof(GetAllParentShopSettingsHandler_ReturnsEmpty_WhenAllShopSettingsAreChildren);
        await using var ctx = CreateContext(dbName);

        var p1 = new Alchemist.Product.Data.ShopSettings { ShopId = 1, ParentSettingsId = 4, Name = "p1", JsonValue = "{}", Type = ShopSettingType.Category };
        var p2 = new Alchemist.Product.Data.ShopSettings { ShopId = 2, Name = "p2", ParentSettingsId = 4, JsonValue = "{}", Type = ShopSettingType.Category };
        var child = new Alchemist.Product.Data.ShopSettings { ParentSettingsId = 1, ShopId = 1, Name = "c", JsonValue = "{}", Type = ShopSettingType.Category };
        await ctx.AddRangeAsync(p1, p2, child);
        await ctx.SaveChangesAsync();

        var handler = new GetAllParentShopSettingsRequestHandler(ctx);

        var list = await handler.Handle(new GetAllParentShopSettingsRequest(), CancellationToken.None);

        Assert.Empty(list);
    }

    [Fact]
    public async Task GetAllParentShopSettingsHandler_ReturnsEmpty_WheNoShopSettings()
    {
        var dbName = nameof(GetAllParentShopSettingsHandler_ReturnsEmpty_WheNoShopSettings);
        await using var ctx = CreateContext(dbName);        

        var handler = new GetAllParentShopSettingsRequestHandler(ctx);

        var list = await handler.Handle(new GetAllParentShopSettingsRequest(), CancellationToken.None);

        Assert.Empty(list);
    }

    [Fact]
    public async Task SaveShopSettingsCommandHandler_DoesNotSave_WhenInvalidData()
    {
        var dbName = nameof(SaveShopSettingsCommandHandler_DoesNotSave_WhenInvalidData);
        await using var ctx = CreateContext(dbName);

        var uow = new EFUnitOfWork<AlchemyContext>(ctx);
        var handler = new SaveShopSettingsCommandHandler(ctx, uow);

        var settings = new Alchemist.Product.Data.ShopSettings { ShopId = 10, Name = "new", Type = ShopSettingType.Category };
        
        var ex = await Assert.ThrowsAsync<DbUpdateException>( ()=> handler.Handle(new SaveShopSettingsCommand(settings), CancellationToken.None));

        var count = await ctx.ShopSettings.CountAsync();

        Assert.Equal(0, count);
    }
}