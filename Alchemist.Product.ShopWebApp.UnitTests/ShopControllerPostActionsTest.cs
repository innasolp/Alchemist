using Alchemist.Common;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Alchemist.Product.ShopWebApp.UnitTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Alchemist.Product.ShopWebApp.UnitTests;

public class ShopControllerPostActionsTest : ShopControllerTest
{    
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

        var result = Assert.IsType<PartialViewResult>(await ShopController.ShopList(shop.Id));
        Assert.Equal("~/Views/Shared/ShopList.cshtml", result.ViewName);
    }

    [Fact]
    public async Task ShopListModelSelectedShopByIdAsync()
    {
        SetupShops();

        var shops = await GetShopsAsync();
        var shopId = shops[new Random().Next(0, shops.Count)].Id;

        var shopTabView = Assert.IsType<PartialViewResult>(await ShopController.ShopList(shopId));

        AssertShopListModel(shopTabView.Model, shops, shopId);
    }

    [Fact]
    public async Task ShopListActionBadRequestWhenShopIdIsNegativeAsync()
    {
        var shopId = new Random().Next(1, int.MaxValue) * -1;
        var result = Assert.IsType<BadRequestObjectResult>(await ShopController.ShopList(shopId));
        Assert.Equal($"Invalid shopId : {shopId}", result.Value);
    }

    #endregion ShopList

    #region Save

    [Fact]
    public async Task SaveActionBadRequestWhenShopIsNullAsync()
    {
        var result = Assert.IsType<BadRequestObjectResult>(await ShopController.Save(null));
        Assert.Equal("shop is null", result.Value);
    }

    [Fact]
    public async Task SaveActionBadRequestWhenShopIdIsNegativeAsync()
    {
        var shop = new Shop { Id = new Random().Next(1, int.MaxValue) * -1 };
        await AssertActionBadRequestWhenInvalidShopIdAsync(shop, (shopController, shop) => shopController.Save(shop));
    }

    [Fact]
    public async Task SaveActionOkShopResultWhenShopIsValidAsync()
    {
        SetupShops();

        var shop = await GetRandomShop();
        SetRandomValues(shop);
        var result = Assert.IsAssignableFrom<OkObjectResult>(await ShopController.Save(shop));
        var savedShop = Assert.IsAssignableFrom<IShop>(result.Value);
        AssertShopsEquals(shop, savedShop);
    }

    #endregion Save

    #region IsChanged

    [Fact]
    public async Task IsChangedActionBadRequestWhenShopIsNullAsync()
    {
        var result = Assert.IsType<BadRequestObjectResult>(await ShopController.IsChanged(null));
        Assert.Equal("shop is null", result.Value);
    }

    [Fact]
    public async Task IsChangedActionBadRequestWhenShopIdIsNegativeAsync()
    {
        var shop = new Shop { Id = new Random().Next(1, int.MaxValue) * -1 };
        await AssertActionBadRequestWhenInvalidShopIdAsync(shop, (shopController, shop) => shopController.IsChanged(shop));
    }

    [Fact]
    public async Task IsChangedActionOkTrueWhenShopIsChangedAsync()
    {
        SetupShops();

        var shop = await GetRandomShop();
        var shopCopy = shop.To<Shop>();
        SetRandomValues(shopCopy);

        var result = Assert.IsAssignableFrom<OkObjectResult>(await ShopController.IsChanged(shopCopy));
        var isChanged = Assert.IsType<bool>(result.Value);
        Assert.True(isChanged);
    }

    [Fact]
    public async Task IsChangedActionOkFalseWhenShopIsNotChangedAsync()
    {
        SetupShops();

        var shop = await GetRandomShop();
        var shopCopy = shop.To<Shop>();

        var result = Assert.IsAssignableFrom<OkObjectResult>(await ShopController.IsChanged(shopCopy));
        var isChanged = Assert.IsType<bool>(result.Value);
        Assert.False(isChanged);
    }

    #endregion IsChanged

    #region Shop

    [Fact]
    public async Task ShopReturnsBadRequestWhenShopIdIsNegative()
    {
        var shopId = -1;
        var result = Assert.IsType<BadRequestObjectResult>(await ShopController.Shop(shopId));
        Assert.Equal($"Invalid shopId : {shopId}", result.Value);
    }

    [Fact]
    public async Task ShopOkEmptyModelWhenShopIdNotExists()
    {
        SetupShops();
        var shopId = int.MaxValue;
        var result = Assert.IsType<PartialViewResult>(await ShopController.Shop(shopId));
        var model = Assert.IsAssignableFrom<IShop>(result.Model);
        Assert.NotEqual(shopId, model.Id);
        Assert.Null(model.Name);
    }

    [Fact]
    public async Task ShopOkWhenShopIdExists()
    {
        SetupShops();
        var shop = await GetRandomShop();
        var result = Assert.IsType<PartialViewResult>(await ShopController.Shop(shop.Id));
        var model = Assert.IsAssignableFrom<IShop>(result.Model);
        Assert.Equal(shop.Id, model.Id);
        Assert.Equal(shop.Name, model.Name);
    }

    #endregion
}
