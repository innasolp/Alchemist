using Alchemist.Product.RestAPI.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using Xunit.Abstractions;
using Moq;
using Alchemist.Product.Interfaces;
using Alchemist.Product.Data;

namespace Alchemist.Product.RestAPI.UnitTest;

public class ShopCategoryControllerGetTest : ControllerTest<ShopCategoryController, ShopCategory>
{
    private readonly List<ShopCategory> _shopCategories =
    [
            new() {Id=1, ItemId = 2, ShopId = 1},
            new() {Id=4, ItemId = 5, ShopId = 1},
            new() {Id=3, ItemId = 3, ShopId = 2}
        ];

    public ShopCategoryControllerGetTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {  
        _mediatr.Setup(r => r.GetShopCategory(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns((int shopId, int itemId, CancellationToken cancellationToken = default) =>
        {
            return Task.FromResult((IShopCategory)_shopCategories.FirstOrDefault(sc => sc.ShopId == shopId && sc.ItemId == itemId));
        });
        
        _mediatr.Setup(r => r.GetShopCategories(It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns((int shopId, CancellationToken cancellationToken = default) =>
        {
            var result = _shopCategories.Where(sc => sc.ShopId == shopId).OfType<IShopCategory>().ToList();
            return Task.FromResult(result);
        });

        _mediatr.Setup(r => r.GetAllCategoryChildren(It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(GetAllCategoryChildren);
    }

    private async Task<List<IShopCategory>> GetAllCategoryChildren(int parentId, CancellationToken cancellationToken = default)
    {
        var children = _shopCategories.Where(c => c.ParentId == parentId).ToList<IShopCategory>();
        var next = new List<IShopCategory>(children);
        children.ForEach(async c => next.AddRange(await GetAllCategoryChildren(c.Id)));

        return await Task.FromResult(next);
    }


    protected override ShopCategoryController CreateController()
    {
        return new ShopCategoryController(_logger, _mediatr.Object, _messageSender.Object);
    }

    [Fact]
    public async Task GetShopCategoryReturnsBadRequestWhenShopIdIsLessThenZeroAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetShopCategoryByShopIdAndItemId(0, 1));
        var badRequest = Assert.IsType<BadRequest<int>>(result.Result);
        Assert.Equal(0, badRequest.Value);
    }

    [Fact]
    public async Task GetShopCategoryReturnsBadRequestWhenItemIdIsLessThenZeroAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetShopCategoryByShopIdAndItemId(1, -2));
        var badRequest = Assert.IsType<BadRequest<int>>(result.Result);
        Assert.Equal(-2, badRequest.Value);
    }

    [Fact]
    public async Task GetShopCategoryReturnsNotFoundWhenShopCategoryNotExistsAsync()
    {        
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetShopCategoryByShopIdAndItemId(55,12));
        var notFound = Assert.IsType<NotFound<Tuple<int, int>>>(result.Result);
        Assert.Equal(55, notFound.Value.Item1);
        Assert.Equal(12, notFound.Value.Item2);
    }

    [Fact]
    public async Task GetShopCategoryReturnsOkWhenShopCategoryExistsAsync()
    {       
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetShopCategoryByShopIdAndItemId(1, 5));
        var ok = Assert.IsType<Ok<ShopCategory>>(result.Result);
        Assert.Equal(1, ok.Value.ShopId);
        Assert.Equal(5, ok.Value.ItemId);
        Assert.Equal(4, ok.Value.Id);
    }

    [Fact]
    public async Task GetShopCategoriesReturnsBadRequestWhenShopIdIsLessThenZeroAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetShopCategories(-2));
        var badRequest = Assert.IsType<BadRequest<int>>(result.Result);
        Assert.Equal(-2, badRequest.Value);
    }

    [Fact]
    public async Task GetShopCategoriesReturnsNotFoundWhenNotExistsAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetShopCategories(10));
        var notFound = Assert.IsType<NotFound<int>>(result.Result);
        Assert.Equal(10, notFound.Value);
    }

    [Fact]
    public async Task GetShopCategoriesReturnsOkWhenExistsAsync()
    { 
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetShopCategories(1));
        var ok = Assert.IsType<Ok<List<ShopCategory>>>(result.Result);
        Assert.Equal(2, ok.Value.Count);
        Assert.Contains(ok.Value, (sc) => sc.Id == 1);
        Assert.Contains(ok.Value, (sc) => sc.Id == 4);
    }

    [Fact]
    public async Task GetAllChildCategoriesSuccess()
    {
        _shopCategories.AddRange(
            [
                new() {Id=5, ItemId = 4, ShopId = 1, ParentId = 1},
                new() {Id=6, ItemId = 6, ShopId = 1, ParentId = 5},
                new() {Id=7, ItemId = 7, ShopId = 2, ParentId = 3},
                new() {Id=8, ItemId = 8, ShopId = 1, ParentId = 6},
                new() {Id=9, ItemId = 9, ShopId = 1, ParentId = 6},
                new() {Id=10, ItemId = 10, ShopId = 1, ParentId = 8},
            ]);

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetAllCategoryChildren(1));
        var ok = Assert.IsType<Ok<List<ShopCategory>>>(result.Result);
        Assert.Equal(5, ok.Value.Count);
    }

    [Fact]
    public async Task GetAllChildCategoriesBadRequestWhenParentIdLessOrEqual0()
    {
        var parentId = -1;
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetAllCategoryChildren(parentId));
        Assert.Equal(parentId, Assert.IsType<BadRequest<int>>(result.Result).Value);
    }

    [Fact]
    public async Task GetAllChildCategoriesNotFoundWhenParentIdNotExists()
    {
        var parentId = _shopCategories.Count + 1;
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetAllCategoryChildren(_shopCategories.Count + 1));
        Assert.Equal(parentId, Assert.IsType<NotFound<int>>(result.Result).Value);
    }
}
