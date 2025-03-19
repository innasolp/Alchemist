using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Alchemist.Product.RestAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit.Abstractions;

namespace Alchemist.Product.RestAPI.UnitTest;

public class ShopCategoryControllerPostTest:ControllerTest<ShopCategoryController, ShopCategory>
{
    public ShopCategoryControllerPostTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _alchemyRepository.Setup(r => r.AddShopCategory(It.IsAny<IShopCategory>())).Returns((IShopCategory shopCategory) =>
        {
            var newShopCategory = shopCategory.To<ShopCategory>();
            newShopCategory.Id += 1;
            return Task.FromResult((IShopCategory)newShopCategory);
        });
    }

    protected override ShopCategoryController CreateController()
    {
        return new ShopCategoryController(_logger, _alchemyRepository.Object, _messageSender.Object);
    }

    [Fact]
    public async Task AddShopCategoryReturnsBadRequestWhenNullAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.AddShopCategory(null));
        Assert.IsType<BadRequest>(result.Result);
    }
    
    [Fact]
    public async Task AddShopCategoryReturnsBadRequestWhenShopIdIsNotValidAsync()
    {
        var shopCategory = new ShopCategory { ShopId = -1, ItemId = 5, Category = "test" };
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.AddShopCategory(shopCategory));
        var badRequest = Assert.IsType<BadRequest<ShopCategory>>(result.Result);
        Assert.Equal(shopCategory.ShopId, badRequest.Value.ShopId);
        Assert.Equal(shopCategory.Category, badRequest.Value.Category);
        Assert.Equal(shopCategory.ItemId, badRequest.Value.ItemId);
    }

    [Fact]
    public async Task AddShopCategoryReturnsBadRequestWhenItemIdIsNotValidAsync()
    {
        var shopCategory = new ShopCategory { ShopId = 1, ItemId = -5, Category = "test" };
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.AddShopCategory(shopCategory));
        var badRequest = Assert.IsType<BadRequest<ShopCategory>>(result.Result);
        Assert.Equal(shopCategory.ShopId, badRequest.Value.ShopId);
        Assert.Equal(shopCategory.Category, badRequest.Value.Category);
        Assert.Equal(shopCategory.ItemId, badRequest.Value.ItemId);
    }
    
    [Fact]
    public async Task AddShopCategoryReturnsBadRequestWhenCategoryIsEmptyAsync()
    {
        var shopCategory = new ShopCategory { ShopId = 1, ItemId = 1, Category = "" };
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.AddShopCategory(shopCategory));
        var badRequest = Assert.IsType<BadRequest<ShopCategory>>(result.Result);
        Assert.Equal(shopCategory.ShopId, badRequest.Value.ShopId);
        Assert.Equal(shopCategory.Category, badRequest.Value.Category);
        Assert.Equal(shopCategory.ItemId, badRequest.Value.ItemId);
    }
    
    [Fact]
    public async Task AddShopCategoryReturnsCreatedWhenIsValidAsync()
    {
        var shopCategory = new ShopCategory { ShopId = 1, ItemId = 1, Category = "test", Id = 0 };
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.AddShopCategory(shopCategory));
        var created = Assert.IsType<Created<ShopCategory>>(result.Result);
        Assert.Equal(shopCategory.ShopId, created.Value.ShopId);
        Assert.Equal(shopCategory.Category, created.Value.Category);
        Assert.Equal(shopCategory.ItemId, created.Value.ItemId);
        Assert.NotEqual(0, created.Value.Id);
    }
}
