using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Alchemist.Product.ShopWebApp.Controllers;
using Alchemist.Product.ShopWebApp.Models;
using Alchemist.Product.ShopWebApp.UnitTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;


namespace Alchemist.Product.ShopWebApp.UnitTests;

public class ShopActionControllerTest 
{
    private readonly Mock<IShopDataService> _shopDataServiceMock = new();

    private readonly ShopActionController _shopController;

    private readonly Mock<HttpContext> _httpContextMock = new();

    protected readonly Mock<ISession> SessionMock = new();

    private readonly Dictionary<string, byte[]> _sessionData = [];

    public ShopActionControllerTest()
    {
        _shopController = new ShopActionController(null, _shopDataServiceMock.Object);

        _shopController.ControllerContext.HttpContext = _httpContextMock.Object;

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

    private void SetupShops()
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

    private async Task<IList<IShop>> GetShopsAsync()
    {
        return await _shopDataServiceMock.Object.GetShops();
    }

    private async Task<IShop> GetRandomShop()
    {
        var shops = await GetShopsAsync();
        return shops[new Random().Next(0, shops.Count)];
    }

    private async Task AssertActionBadRequestWhenInvalidShopIdAsync(ShopModel shop,
        Func<ShopActionController, ShopModel, Task<IActionResult>> action)
    {
        var result = Assert.IsType<BadRequestObjectResult>(await action(_shopController, shop));
        Assert.Equal($"Invalid shopId : {shop.Id}", result.Value);
    }

    private static void AssertShopListModel(object model, IList<IShop> shops, int? shopId = null)
    {
        var shopListDefinition = new[] { new { ShopName = "", HRef = "", IsSelected = false, Id = 0 } };

        var shopListModel = JsonExtensions.DeserializeAnonymousType(model, shopListDefinition);

        Assert.Equal(shops.Count, shopListModel?.Length);
        Assert.True(shopListModel?.All(shopItem => shops.Any(s => s.Name == shopItem.ShopName)));

        if (shopId != null)
        {
            if (shopId != 0)
                Assert.Equal(shopId, shopListModel?.FirstOrDefault(s => s.IsSelected)?.Id);
            else
                Assert.True(shopListModel?.All(s => !s.IsSelected));
        }
    }

    private static void SetRandomValues(IShop shop)
    {
        shop.Name = Guid.NewGuid().ToString();
        shop.Url = Guid.NewGuid().ToString();
        shop.Caption = Guid.NewGuid().ToString();
    }

    private static void AssertShopsEquals(IShop expected, IShop result)
    {
        Assert.Equal(expected.Id, result.Id);
        Assert.Equal(expected.Name, result.Name);
        Assert.Equal(expected.Url, result.Url);
        Assert.True((string.IsNullOrEmpty(expected.Caption) && string.IsNullOrEmpty(result.Caption))
            || result.Caption.Equals(expected.Caption));
    }    

    #region ShopList

    [Fact]
    public async Task ShopListActionSuccessResultIsShopListPartialViewAsync()
    {
        SetupShops();

        SessionMock.Object.SetInt32("shops_uploaded", 1);

        var shop = await GetRandomShop();

        var result = Assert.IsType<PartialViewResult>(await _shopController.ShopList(shop.Id));
        Assert.Equal("~/Views/Shared/ShopList.cshtml", result.ViewName);
    }

    [Fact]
    public async Task ShopListModelSelectedShopByIdAsync()
    {
        SetupShops();

        var shops = await GetShopsAsync();
        var shopId = shops[new Random().Next(0, shops.Count)].Id;

        var shopTabView = Assert.IsType<PartialViewResult>(await _shopController.ShopList(shopId));

        AssertShopListModel(shopTabView.Model, shops, shopId);
    }

    [Fact]
    public async Task ShopListActionBadRequestWhenShopIdIsNegativeAsync()
    {
        var shopId = new Random().Next(1, int.MaxValue) * -1;
        var result = Assert.IsType<BadRequestObjectResult>(await _shopController.ShopList(shopId));
        Assert.Equal($"Invalid shopId : {shopId}", result.Value);
    }

    #endregion ShopList

    #region Save

    [Fact]
    public async Task SaveActionBadRequestWhenShopIsNullAsync()
    {
        var result = Assert.IsType<BadRequestObjectResult>(await _shopController.Save(null));
        Assert.Equal("shop is null", result.Value);
    }

    [Fact]
    public async Task SaveActionBadRequestWhenShopIdIsNegativeAsync()
    {
        var shop = new ShopModel { Id = new Random().Next(1, int.MaxValue) * -1 };
        await AssertActionBadRequestWhenInvalidShopIdAsync(shop, (shopController, shop) => shopController.Save(shop));
    }

    [Fact]
    public async Task SaveActionOkShopResultWhenShopIsValidAsync()
    {
        SetupShops();

        var shop = await GetRandomShop();
        SetRandomValues(shop);
        var result = Assert.IsAssignableFrom<OkObjectResult>(await _shopController.Save(shop.To<ShopModel>()));
        var savedShop = Assert.IsAssignableFrom<IShop>(result.Value);
        AssertShopsEquals(shop, savedShop);
    }

    #endregion Save

    #region IsChanged

    [Fact]
    public async Task IsChangedActionBadRequestWhenShopIsNullAsync()
    {
        var result = Assert.IsType<BadRequestObjectResult>(await _shopController.IsChanged(null));
        Assert.Equal("shop is null", result.Value);
    }

    [Fact]
    public async Task IsChangedActionBadRequestWhenShopIdIsNegativeAsync()
    {
        var shop = new ShopModel { Id = new Random().Next(1, int.MaxValue) * -1 };
        await AssertActionBadRequestWhenInvalidShopIdAsync(shop, (shopController, shop) => shopController.IsChanged(shop));
    }

    [Fact]
    public async Task IsChangedActionOkTrueWhenShopIsChangedAsync()
    {
        SetupShops();

        var shop = await GetRandomShop();
        var shopCopy = shop.To<ShopModel>();
        SetRandomValues(shopCopy);

        var result = Assert.IsAssignableFrom<OkObjectResult>(await _shopController.IsChanged(shopCopy));
        var isChanged = Assert.IsType<bool>(result.Value);
        Assert.True(isChanged);
    }

    [Fact]
    public async Task IsChangedActionOkFalseWhenShopIsNotChangedAsync()
    {
        SetupShops();

        var shop = await GetRandomShop();
        var shopCopy = shop.To<Shop>();

        var result = Assert.IsAssignableFrom<OkObjectResult>(await _shopController.IsChanged(shopCopy.To<ShopModel>()));
        var isChanged = Assert.IsType<bool>(result.Value);
        Assert.False(isChanged);
    }

    #endregion IsChanged

    #region Shop

    [Fact]
    public async Task ShopReturnsBadRequestWhenShopIdIsNegative()
    {
        var shopId = -1;
        var result = Assert.IsType<BadRequestObjectResult>(await _shopController.Shop(shopId));
        Assert.Equal($"Invalid shopId : {shopId}", result.Value);
    }

    [Fact]
    public async Task ShopOkEmptyModelWhenShopIdNotExists()
    {
        SetupShops();
        var shopId = int.MaxValue;
        var result = Assert.IsType<PartialViewResult>(await _shopController.Shop(shopId));
        var model = Assert.IsAssignableFrom<IShop>(result.Model);
        Assert.NotEqual(shopId, model.Id);
        Assert.Null(model.Name);
    }

    [Fact]
    public async Task ShopOkWhenShopIdExists()
    {
        SetupShops();
        var shop = await GetRandomShop();
        var result = Assert.IsType<PartialViewResult>(await _shopController.Shop(shop.Id));
        var model = Assert.IsAssignableFrom<IShop>(result.Model);
        Assert.Equal(shop.Id, model.Id);
        Assert.Equal(shop.Name, model.Name);
    }

    #endregion
}
