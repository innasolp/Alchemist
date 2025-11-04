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
    private static void AssertIndexModel(object model, int? shopId = null)
    {
        var indexModelDefinition = new
        {
            ShopId = int.MinValue
        };
        var indexModel = JsonExtensions.DeserializeAnonymousType(model, indexModelDefinition);

        if (shopId != null)
            Assert.Equal(shopId, indexModel.ShopId);
        else
            Assert.Equal(0, indexModel.ShopId);
    }

    #region Index

    [Fact]
    public void IndexActionIsViewResult()
    {
        SetupShops();
        var result = Assert.IsType<ViewResult>(ShopController.Index());
        Assert.Equal("~/Views/Home/Index.cshtml", result.ViewName);
    }

    [Fact]
    public void IndexActionModelShopsNotLoadedOnStart()
    {
        SetupShops();
        var result = Assert.IsType<ViewResult>(ShopController.Index());       

        var indexModel = JsonExtensions.DeserializeAnonymousType(result.Model, new { ShopsUploaded = true });
        Assert.False(indexModel?.ShopsUploaded);
    }    

    #endregion Index

    #region IndexFromQuery

    [Fact]
    public void IndexFromQueryActionReturnsBadRequestWhenShopIdIsNegative()
    {
        AssertActionBadRequestWhenInvalidShopId(new Random().Next(1, int.MaxValue) * -1, (controller, shopId)=>controller.IndexFromQuery(shopId));
    }

    [Fact]
    public void IndexFromQueryActionReturnsBadRequestWhenShopIdIsZero()
    {
        AssertActionBadRequestWhenInvalidShopId(0, (controller, shopId) => controller.IndexFromQuery(shopId));
    }

    [Fact]
    public async Task IndexFromQueryActionIsIndexViewWhenSuccessAsync()
    {
        SetupShops();
        
        var shop = await GetRandomShop();

        var indexView = Assert.IsType<ViewResult>(ShopController.IndexFromQuery(shop.Id));
        Assert.Equal("~/Views/Home/Index.cshtml", indexView.ViewName);
    }

    [Fact]
    public async Task IndexFromQueryModelSelectedShopByIdAsync()
    {
        var shopId = 5;
        var indexView = Assert.IsType<ViewResult>(ShopController.IndexFromQuery(shopId));

        AssertIndexModel(indexView.Model, shopId);
    }

    #endregion IndexFromQuery

    #region IndexRoute

    [Fact]
    public void IndexRouteActionReturnsBadRequestWhenShopIdIsNegative()
    {
        AssertActionBadRequestWhenInvalidShopId(new Random().Next(1, int.MaxValue) * -1, (controller, shopId) => controller.IndexRoute(shopId));
    }    

    [Fact]
    public async Task IndexRouteActionIsIndexViewWhenSuccessAsync()
    {
        SetupShops();

        var shop = await GetRandomShop();

        var indexView =Assert.IsType<ViewResult>(ShopController.IndexRoute(shop.Id));

        Assert.Equal("~/Views/Home/Index.cshtml", indexView.ViewName);
    }

    [Fact]
    public void IndexRouteyModelSelectedShopById()
    {
        var shopId = 3;

        var indexView = Assert.IsType<ViewResult>(ShopController.IndexRoute(shopId));

        AssertIndexModel(indexView.Model, shopId);
    }

    #endregion IndexRoute

    #region New

    [Fact]
    public void NewActionIsIndexView()
    {
        var indexView = Assert.IsType<ViewResult>(ShopController.New());

        Assert.Equal("~/Views/Home/Index.cshtml", indexView.ViewName);
    }

    [Fact]
    public void NewActionModelShopNotSelected()
    {
        var indexView = Assert.IsType<ViewResult>(ShopController.New());

        AssertIndexModel(indexView.Model, 0);
    }

    #endregion New
}