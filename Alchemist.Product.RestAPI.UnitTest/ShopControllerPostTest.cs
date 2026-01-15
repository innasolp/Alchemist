using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using Moq;
using Alchemist.Product.Interfaces;
using Xunit.Abstractions;
using Alchemist.Product.RestAPI.Controllers;

namespace Alchemist.Product.RestAPI.UnitTest;

public class ShopControllerPostTest: ControllerTest<ShopController, Data.Shop>
{
    protected override ShopController CreateController() => new ShopController(_logger, _mediatr.Object, _messageSender.Object);

    public ShopControllerPostTest(ITestOutputHelper testOutputHelper):base(testOutputHelper)
    {
        _mediatr.Setup(r => r.CreateShop(It.IsAny<Data.Shop>(), It.IsAny<CancellationToken>())).Returns(CreateShop);
        _mediatr.Setup(r => r.UpdateShop(It.IsAny<Data.Shop>(), It.IsAny<CancellationToken>())).Returns(UpdateShop);
    }

    private async Task<IShop> CreateShop(IShop shop, CancellationToken cancellationToken = default)
    {
        var newShop = shop.To<Data.Shop>();
        newShop.Id += 1;
        return await Task.FromResult(newShop);
    }
    
    private async Task<IShop> UpdateShop(IShop shop, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(shop.To<Data.Shop>());
    }

    [Fact]
    public async Task CreateShopReturnsBadRequestWhenShopIsNullAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.CreateShop(null));
        Assert.IsType<BadRequest>(result.Result);
    }

    [Fact]
    public async Task CreateShopReturnsBadRequestWhenShopNameIsEmptyAsync()
    {
        var shop = new Data.Shop
        {
            Url = Guid.NewGuid().ToString(),
            Id = 1
        };
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.CreateShop(shop));
        var badRequest = Assert.IsType<BadRequest<Data.Shop>>(result.Result);
        Assert.Equal(shop.Id, badRequest.Value.Id);
    }

    [Fact]
    public async Task CreateShopReturnsBadRequestWhenShopUrlIsEmptyAsync()
    {
        var shop = new Data.Shop
        {
            Name = Guid.NewGuid().ToString(),
            Id = 1
        };
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.CreateShop(shop));
        var badRequest = Assert.IsType<BadRequest<Data.Shop>>(result.Result);
        Assert.Equal(shop.Id, badRequest.Value.Id);
    }    

    [Fact]
    public async Task CreateShopReturnsCreatedWhenShopIsValidAsync()
    {
        var shop = new Data.Shop
        {
            Name = Guid.NewGuid().ToString(),
            Url = Guid.NewGuid().ToString(),
            Id = 0
        };
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.CreateShop(shop));
        var badRequest = Assert.IsType<Created<Data.Shop>>(result.Result);
        Assert.NotEqual(shop.Id, badRequest.Value.Id);
        Assert.Equal(shop.Name, badRequest.Value.Name);
        Assert.Equal(shop.Url, badRequest.Value.Url);
    }

    [Fact]
    public async Task UpdateShopReturnsBadRequestWhenShopIdLessOrEqualsThenZeroAsync()
    {
        var shop = new Data.Shop
        {
            Name = Guid.NewGuid().ToString(),
            Url = Guid.NewGuid().ToString(),
            Id = -5
        };
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.UpdateShop(shop));
        var badRequest = Assert.IsType<BadRequest<Data.Shop>>(result.Result);
        Assert.Equal(shop.Id, badRequest.Value.Id);
    }

    [Fact]
    public async Task UpdateShopReturnsBadRequestWhenShopIsNullAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.UpdateShop(null));
        Assert.IsType<BadRequest>(result.Result);
    }

    [Fact]
    public async Task UpdateShopReturnsBadRequestWhenShopNameIsEmptyAsync()
    {
        var shop = new Data.Shop
        {
            Url = Guid.NewGuid().ToString(),
            Id = 1
        };
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.UpdateShop(shop));
        var badRequest = Assert.IsType<BadRequest<Data.Shop>>(result.Result);
        Assert.Equal(shop.Id, badRequest.Value.Id);
    }

    [Fact]
    public async Task UpdateShopReturnsBadRequestWhenShopUrlIsEmptyAsync()
    {
        var shop = new Data.Shop
        {
            Name = Guid.NewGuid().ToString(),
            Id = 1
        };
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.UpdateShop(shop));
        var badRequest = Assert.IsType<BadRequest<Data.Shop>>(result.Result);
        Assert.Equal(shop.Id, badRequest.Value.Id);
    }

    [Fact]
    public async Task UpdateShopReturnsAcceptedWhenShopIsValidAsync()
    {
        var shop = new Data.Shop
        {
            Name = Guid.NewGuid().ToString(),
            Url = Guid.NewGuid().ToString(),
            Id = new Random().Next(100)
        };
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.UpdateShop(shop));
        var badRequest = Assert.IsType<Accepted<Data.Shop>>(result.Result);
        Assert.Equal(shop.Id, badRequest.Value.Id);
        Assert.Equal(shop.Name, badRequest.Value.Name);
        Assert.Equal(shop.Url, badRequest.Value.Url);
    }
}
