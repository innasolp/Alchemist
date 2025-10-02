using Alchemist.Common;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Alchemist.Product.ShopWebApp.Controllers;
using Alchemist.Product.ShopWebApp.UnitTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.ShopWebApp.UnitTests;

public class ShopControllerGetActionsTest : ShopControllerTest
{
    private static void AssertIndexModel(object model, bool? shopsUploaded = true, IList<IShop>? shops = null, int? shopId = null)
    {
        var indexModelDefinition = new
        {
            ShopsUploaded = true,
            ShopTab = new
            {
                CurrentShopModel = new Shop(),
                ShopItems = new[] { new { ShopName = "", HRef = "", IsSelected = false, Id = 0 } }
            }
        };
        var indexModel = JsonExtensions.DeserializeAnonymousType(model, indexModelDefinition);
        
        if(shopsUploaded != null)
            Assert.Equal(shopsUploaded, indexModel?.ShopsUploaded);

        if (shops != null)
        {
            Assert.Equal(shops.Count, indexModel?.ShopTab.ShopItems.Length);
            Assert.True(indexModel?.ShopTab.ShopItems.All(shopItem => shops.Any(s => s.Name == shopItem.ShopName)));
        }

        if (shopId != null)
        {
            Assert.Equal(shopId, indexModel.ShopTab.CurrentShopModel?.Id);

            if (shopId != 0)
                Assert.Equal(shopId, indexModel?.ShopTab.ShopItems.FirstOrDefault(s => s.IsSelected)?.Id);
            else
                Assert.True(indexModel?.ShopTab.ShopItems.All(s => !s.IsSelected));
        }
    }

    #region Index

    [Fact]
    public async Task IndexActionIsViewResultAsync()
    {
        SetupShops();
        var result = Assert.IsType<ViewResult>(await ShopController.Index());
        Assert.Equal("~/Views/Home/Index.cshtml", result.ViewName);
    }

    [Fact]
    public async Task IndexActionModelShopsNotLoadedOnStartAsync()
    {
        SetupShops();
        var result = Assert.IsType<ViewResult>(await ShopController.Index());       

        var indexModel = JsonExtensions.DeserializeAnonymousType(result.Model, new { ShopsUploaded = true });
        Assert.False(indexModel?.ShopsUploaded);
    }

    [Fact]
    public async Task IndexActionModelContainsShopsWhenShopsAlreadyUploadedAsync()
    {
        SetupShops();

        SessionMock.Object.SetInt32("shops_uploaded", 1);

        var result = Assert.IsType<ViewResult>(await ShopController.Index());

        var shops = await GetShopsAsync();

        AssertIndexModel(result.Model, true, shops);
    }

    #endregion Index

    #region IndexFromQuery

    [Fact]
    public async Task IndexFromQueryActionReturnsBadRequestWhenShopIdIsNegativeAsync()
    {
        await AssertActionBadRequestWhenInvalidShopIdAsync(new Random().Next(1, int.MaxValue) * -1, (controller, shopId)=>controller.IndexFromQuery(shopId));
    }

    [Fact]
    public async Task IndexFromQueryActionReturnsBadRequestWhenShopIdIsZeroAsync()
    {
        await AssertActionBadRequestWhenInvalidShopIdAsync(0, (controller, shopId) => controller.IndexFromQuery(shopId));
    }

    [Fact]
    public async Task IndexFromQueryActionReturnsNotFoundWhenNotExistingShopIdAsync()
    {
        SetupShops();

        await AssertActionNotFoundWhenNotExistingShopIdAsync((await GetShopsAsync()).Count + 1,
            (controller, shopId) => controller.IndexFromQuery(shopId));
    }

    [Fact]
    public async Task IndexFromQueryActionIsIndexViewWhenSuccessAsync()
    {
        SetupShops();
        
        var shop = await GetRandomShop();

        var indexView = Assert.IsType<ViewResult>(await ShopController.IndexFromQuery(shop.Id));
        Assert.Equal("~/Views/Home/Index.cshtml", indexView.ViewName);
    }

    [Fact]
    public async Task IndexFromQueryModelSelectedShopByIdAsync()
    {
        SetupShops();

        var shops = await GetShopsAsync();
        var shopId = shops[new Random().Next(0, shops.Count)].Id;

        var indexView = Assert.IsType<ViewResult>(await ShopController.IndexFromQuery(shopId));

        AssertIndexModel(indexView.Model, true, shops, shopId);
    }

    #endregion IndexFromQuery

    #region IndexRoute

    [Fact]
    public async Task IndexRouteActionReturnsBadRequestWhenShopIdIsNegativeAsync()
    {
        await AssertActionBadRequestWhenInvalidShopIdAsync(new Random().Next(1, int.MaxValue) * -1, (controller, shopId) => controller.IndexRoute(shopId));
    }

    [Fact]
    public async Task IndexRouteActionReturnsBadRequestWhenShopIdIsZeroAsync()
    {
        await AssertActionBadRequestWhenInvalidShopIdAsync(0, (controller, shopId) => controller.IndexRoute(shopId));
    }

    [Fact]
    public async Task IndexRouteActionReturnsNotFoundWhenNotExistingShopIdAsync()
    {
        SetupShops();

        await AssertActionNotFoundWhenNotExistingShopIdAsync((await GetShopsAsync()).Count + 1,
            (controller, shopId) => controller.IndexRoute(shopId));
    }


    [Fact]
    public async Task IndexRouteActionIsIndexViewWhenSuccessAsync()
    {
        SetupShops();

        var shop = await GetRandomShop();

        var indexView =Assert.IsType<ViewResult>(await ShopController.IndexRoute(shop.Id));

        Assert.Equal("~/Views/Home/Index.cshtml", indexView.ViewName);
    }

    [Fact]
    public async Task IndexRouteyModelSelectedShopByIdAsync()
    {
        SetupShops();

        var shops = await GetShopsAsync();
        var shopId = shops[new Random().Next(0, shops.Count)].Id;

        var indexView = Assert.IsType<ViewResult>(await ShopController.IndexRoute(shopId));

        AssertIndexModel(indexView.Model, true, shops, shopId);
    }

    #endregion IndexRoute

    #region New

    [Fact]
    public async Task NewActionIsIndexViewAsync()
    {
        SetupShops();       

        var indexView = Assert.IsType<ViewResult>(await ShopController.New());

        Assert.Equal("~/Views/Home/Index.cshtml", indexView.ViewName);
    }

    [Fact]
    public async Task NewActionModelShopNotSelectedAsync()
    {
        SetupShops();

        var shops = await GetShopsAsync();

        var indexView = Assert.IsType<ViewResult>(await ShopController.New());

        AssertIndexModel(indexView.Model, null, shops, 0);
    }

    #endregion New
}