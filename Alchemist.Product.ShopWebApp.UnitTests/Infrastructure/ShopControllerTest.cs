using Alchemist.DataService.Interfaces;
using Alchemist.Product.Interfaces;
using Alchemist.Product.ShopWebApp.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Alchemist.Product.ShopWebApp.UnitTests.Infrastructure;

public abstract class ShopControllerTest
{
    private readonly Mock<IShopDataService> _shopDataServiceMock = new();

    protected readonly ShopController ShopController;

    private readonly Mock<HttpContext> _httpContextMock = new();

    protected readonly Mock<ISession> SessionMock = new();

    private readonly Dictionary<string, byte[]> _sessionData = [];

    public ShopControllerTest()
    {
        ShopController = new ShopController(null, _shopDataServiceMock.Object);

        ShopController.ControllerContext.HttpContext = _httpContextMock.Object;

        _httpContextMock.Setup(c => c.Session).Returns(SessionMock.Object);

        byte[]? bytes;
        SessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out bytes)).Returns((string key, out byte[]? result) =>
        {
            return _sessionData.TryGetValue(key, out result);
        });
        SessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>())).Callback((string key, byte[] value) =>
        {
            _sessionData.TryAdd(key, value);
        });
    }

    protected void SetupShops()
    {
        _shopDataServiceMock.Reset();

        var shops = TestRepository.GetShopsTestData(new Random().Next(2, 10)).OfType<IShop>().ToList();

        int i = 0;
        foreach (IShop shop in shops)
        {
            i++;
            shop.Id = i;
        }

        _shopDataServiceMock.Setup(s => s.GetShops()).Returns(Task.FromResult(shops));
        _shopDataServiceMock.Setup(s => s.GetShop(It.IsAny<int>())).
            Returns((int id) => Task.FromResult(shops.FirstOrDefault(s => s.Id == id)));
        _shopDataServiceMock.Setup(s => s.CreateShop(It.IsAny<IShop>())).ReturnsAsync((IShop shop) =>
        {
            shop.Id = shops.Count + 1;
            shops.Add(shop);
            return shop;
        });
        _shopDataServiceMock.Setup(s => s.UpdateShop(It.IsAny<IShop>())).ReturnsAsync((IShop shop) =>
        {
            var currentShop = shops.FirstOrDefault(s => s.Id == shop.Id);
            if (currentShop == null) return default;
            currentShop.Name = shop.Name;
            currentShop.Url = shop.Url;
            currentShop.Caption = shop.Caption;
            return currentShop;
        });
    }

    protected async Task<IList<IShop>> GetShopsAsync()
    {
        return await _shopDataServiceMock.Object.GetShops();
    }

    protected async Task<IShop> GetRandomShop()
    {
        var shops = await GetShopsAsync();
        return shops[new Random().Next(0, shops.Count)];
    }

    protected async Task AssertActionBadRequestWhenInvalidShopIdAsync(int shopId,
        Func<ShopController, int, Task<IActionResult>> action)
    {
        var result = Assert.IsType<BadRequestObjectResult>(await action(ShopController, shopId));
        Assert.Equal($"Invalid shopId : {shopId}", result.Value);
    }

    protected async Task AssertActionBadRequestWhenInvalidShopIdAsync(IShop shop,
        Func<ShopController, IShop, Task<IActionResult>> action)
    {
        var result = Assert.IsType<BadRequestObjectResult>(await action(ShopController, shop));
        Assert.Equal($"Invalid shopId : {shop.Id}", result.Value);
    }
    

    protected async Task AssertActionNotFoundWhenNotExistingShopIdAsync(int shopId,
        Func<ShopController, int, Task<IActionResult>> Action)
    {
        var result = Assert.IsType<NotFoundObjectResult>(await Action(ShopController, shopId));
        Assert.Equal($"Shop with id={shopId} not found.", result.Value);
    }
}
