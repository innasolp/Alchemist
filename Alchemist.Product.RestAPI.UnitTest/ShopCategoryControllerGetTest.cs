using Alchemist.Product.RestAPI.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using Xunit.Abstractions;
using Moq;
using Alchemist.Product.Interfaces;
using Alchemist.Product.Entities;

namespace Alchemist.Product.RestAPI.UnitTest;

public class ShopCategoryControllerGetTest : ControllerTest<ShopCategoryController, ShopCategory>
{
    public ShopCategoryControllerGetTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        var shopCategories = new List<ShopCategory>
        {
            new() {Id=1, ItemId = 2, ShopId = 1},
            new() {Id=4, ItemId = 5, ShopId = 1},
            new() {Id=3, ItemId = 3, ShopId = 2}
        };

        _alchemyRepository.Setup(r => r.GetShopCategory(It.IsAny<int>(), It.IsAny<int>())).Returns((int shopId, int itemId) =>
        {
            return Task.FromResult((IShopCategory)shopCategories.FirstOrDefault(sc => sc.ShopId == shopId && sc.ItemId == itemId));
        });
        
        _alchemyRepository.Setup(r => r.GetShopCategories(It.IsAny<int>())).Returns((int shopId) =>
        {
            var result = shopCategories.Where(sc => sc.ShopId == shopId).OfType<IShopCategory>().ToList();
            return Task.FromResult(result);
        });
    }

    protected override ShopCategoryController CreateController()
    {
        return new ShopCategoryController(_logger, _alchemyRepository.Object, _messageSender.Object);
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
}
