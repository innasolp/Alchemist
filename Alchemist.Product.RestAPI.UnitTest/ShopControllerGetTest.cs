using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Alchemist.Product.RestAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit.Abstractions;

namespace Alchemist.Product.RestAPI.UnitTest;

public class ShopControllerGetTest(ITestOutputHelper testOutputHelper) : ControllerTest<ShopController, Shop>(testOutputHelper)
{
    protected override ShopController CreateController() => new(_logger, _alchemyRepository.Object, _messageSender.Object);

    [Fact]
    public async Task GetShopByUrlReturnsBadRequestWhenShopNameIsEmptyAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetShopByUrl(""));
        Assert.IsType<BadRequest>(result.Result);
    }

    [Fact]
    public async Task GetShopByUrlReturnsNotFoundWhenShopNameNotExistsAsync()
    {
        var setup = _alchemyRepository.Setup(r => r.GetShopByUrl(It.IsAny<string>(), It.IsAny<CancellationToken>()));
        setup.Returns(Task.FromResult(default(IShop)));

        var url = "http://test";
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetShopByUrl(url));
        var notFound = Assert.IsType<NotFound<string>>(result.Result);
        Assert.Equal(url, notFound.Value);
    }

    [Fact]
    public async Task GetShopByUrlReturnsOkWhenShopExistsAsync()
    {
        var url = "http://test";
        var shop = new Shop() { Name = "test", Id= new Random().Next(100), Url = url };

        var setup = _alchemyRepository.Setup(r => r.GetShopByUrl(url, It.IsAny<CancellationToken>()));
        setup.Returns(Task.FromResult((IShop)shop));
        
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetShopByUrl(url));
        var ok = Assert.IsType<Ok<Shop>>(result.Result);
        Assert.Equal(shop.Id, ok.Value.Id);
        Assert.Equal(shop.Name, ok.Value.Name);
        Assert.Equal(shop.Url, ok.Value.Url);
    }

    [Fact]
    public async Task GetShopByNameReturnsBadRequestWhenShopNameIsEmptyAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetShopByName(""));
        Assert.IsType<BadRequest>(result.Result);
    }

    [Fact]
    public async Task GetShopByNameReturnsNotFoundWhenShopNameNotExistsAsync()
    {
        var setup = _alchemyRepository.Setup(r => r.GetShopByName(It.IsAny<string>(), It.IsAny<CancellationToken>()));
        setup.Returns(Task.FromResult(default(IShop)));

        var name = "test";
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetShopByName(name));
        var notFound = Assert.IsType<NotFound<string>>(result.Result);
        Assert.Equal(name, notFound.Value);
    }

    [Fact]
    public async Task GetShopByNameReturnsOkWhenShopExistsAsync()
    {
        var name = "test";
        var shop = new Shop() { Name = name, Id = new Random().Next(100) };

        var setup = _alchemyRepository.Setup(r => r.GetShopByName(name, It.IsAny<CancellationToken>()));
        setup.Returns(Task.FromResult((IShop)shop));

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetShopByName(name));
        var ok = Assert.IsType<Ok<Shop>>(result.Result);
        Assert.Equal(shop.Id, ok.Value.Id);
        Assert.Equal(shop.Name, ok.Value.Name);
    }

    [Fact]
    public async Task GetShopByIdReturnsBadRequestWhenShopIdIsNullAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetShop(0));
        var badRequest = Assert.IsType<BadRequest<int>>(result.Result);
        Assert.Equal(0, badRequest.Value);
    }
    [Fact]
    public async Task GetShopByIdReturnsBadRequestWhenShopIdIsNegativeAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetShop(-1));
        var badRequest = Assert.IsType<BadRequest<int>>(result.Result);
        Assert.Equal(-1, badRequest.Value);
    }

    [Fact]
    public async Task GetShopByIdReturnsNotFoundWhenShopIdNotExistsAsync()
    {
        var setup = _alchemyRepository.Setup(r => r.GetShop(It.IsAny<int>(), It.IsAny<CancellationToken>()));
        setup.Returns(Task.FromResult(default(IShop)));

        var id = new Random().Next(1000);
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetShop(id));
        var notFound = Assert.IsType<NotFound<int>>(result.Result);
        Assert.Equal(id, notFound.Value);
    }

    [Fact]
    public async Task GetShopByIdReturnsOkWhenShopExistsAsync()
    {
        var name = "test";
        var shop = new Shop() { Name = name, Id = new Random().Next(100) };

        var setup = _alchemyRepository.Setup(r => r.GetShop(shop.Id, It.IsAny<CancellationToken>()));
        setup.Returns(Task.FromResult((IShop)shop));

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetShop(shop.Id));
        var ok = Assert.IsType<Ok<Shop>>(result.Result);
        Assert.Equal(shop.Id, ok.Value.Id);
        Assert.Equal(shop.Name, ok.Value.Name);
    }

    [Fact]
    public async Task GetShopsReturnsNotFoundWhenNoShopsAsync()
    {
        var setup = _alchemyRepository.Setup(r => r.GetShops(It.IsAny<CancellationToken>()));
        setup.Returns(Task.FromResult(default(List<IShop>)));
       
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetShops());
        Assert.IsType<NotFound>(result.Result);       
    }

    [Fact]
    public async Task GetShopsReturnsOkWhenShopsAnyAsync()
    {
        var shops = new List<IShop>{
            new Shop() { Name = Guid.NewGuid().ToString(), Id = new Random().Next(100) },
            new Shop() { Name = Guid.NewGuid().ToString(), Id = new Random().Next(100) }
         };

        var setup = _alchemyRepository.Setup(r => r.GetShops(It.IsAny<CancellationToken>()));
        setup.Returns(Task.FromResult(shops));

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await Controller.GetShops());
        var ok = Assert.IsType<Ok<List<Shop>>>(result.Result);
        Assert.Equal(shops.Count, ok.Value.Count);
        Assert.Equal(shops, ok.Value, (s1, s2) => s1.Id == s2.Id);
    }
}