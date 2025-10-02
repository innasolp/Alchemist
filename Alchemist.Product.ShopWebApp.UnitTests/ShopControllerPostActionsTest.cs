using Alchemist.Common;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Alchemist.Product.ShopWebApp.UnitTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Alchemist.Product.ShopWebApp.UnitTests;

public class ShopControllerPostActionsTest : ShopControllerTest
{
    private static void AssertShopTabModel(object model, IList<IShop>? shops = null, int? shopId = null)
    {
        var shopTabDefinition = new
        {
            CurrentShopModel = new Shop(),
            ShopItems = new[] { new { ShopName = "", HRef = "", IsSelected = false, Id = 0 } }
        };

        var shopTabModel = JsonExtensions.DeserializeAnonymousType(model, shopTabDefinition);

        if (shops != null)
        {
            Assert.Equal(shops.Count, shopTabModel?.ShopItems.Length);
            Assert.True(shopTabModel?.ShopItems.All(shopItem => shops.Any(s => s.Name == shopItem.ShopName)));
        }

        if (shopId != null)
        {
            Assert.Equal(shopId, shopTabModel.CurrentShopModel?.Id);

            if (shopId != 0)
                Assert.Equal(shopId, shopTabModel?.ShopItems.FirstOrDefault(s => s.IsSelected)?.Id);
            else
                Assert.True(shopTabModel?.ShopItems.All(s => !s.IsSelected));
        }
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

    #region ShopTab

    [Fact]
    public async Task ShopTabActionSuccessResultIsShopTabPartialViewAsync()
    {
        SetupShops();

        SessionMock.Object.SetInt32("shops_uploaded", 1);

        var shop = await GetRandomShop();

        var result = Assert.IsType<PartialViewResult>(await ShopController.ShopTab(shop.Id));
        Assert.Equal("~/Views/Shared/ShopTab.cshtml", result.ViewName);
    }

    [Fact]
    public async Task ShopTabModelSelectedShopByIdAsync()
    {
        SetupShops();

        var shops = await GetShopsAsync();
        var shopId = shops[new Random().Next(0, shops.Count)].Id;

        var shopTabView = Assert.IsType<PartialViewResult>(await ShopController.ShopTab(shopId));

        AssertShopTabModel(shopTabView.Model, shops, shopId);
    }

    [Fact]
    public async Task ShopTabActionBadRequestWhenShopIdIsNegativeAsync()
    {
        var shopId = new Random().Next(1, int.MaxValue) * -1;
        await AssertActionBadRequestWhenInvalidShopIdAsync(shopId, (shopController, shopId) => shopController.ShopTab(shopId));
    }

    [Fact]
    public async Task ShopTabActionBadRequestWhenShopIdIsZeroAsync()
    {
        await AssertActionBadRequestWhenInvalidShopIdAsync(0, (shopController, shopId) => shopController.ShopTab(shopId));
    }

    [Fact]
    public async Task ShopTabActionNotFoundWhenNotExistingShopAsync()
    {
        SetupShops();
        await AssertActionNotFoundWhenNotExistingShopIdAsync((await GetShopsAsync()).Count + 1, (shopController, shopId) => shopController.ShopTab(shopId));
    }

    #endregion ShopTab

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
        await AssertActionBadRequestWhenInvalidShopIdAsync(shopId, (shopController, shopId) => shopController.ShopList(shopId));
    }

    [Fact]
    public async Task ShopListActionBadRequestWhenShopIdIsZeroAsync()
    {
        await AssertActionBadRequestWhenInvalidShopIdAsync(0, (shopController, shopId) => shopController.ShopList(shopId));
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
}
