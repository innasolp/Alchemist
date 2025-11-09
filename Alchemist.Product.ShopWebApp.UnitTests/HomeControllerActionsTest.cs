using Alchemist.Common;
using Alchemist.Product.ShopWebApp.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.ShopWebApp.UnitTests;

public class HomeControllerActionsTest 
{
    private readonly HomeController _homeController = new HomeController(null);

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
        var result = Assert.IsType<ViewResult>(_homeController.Index());
        Assert.Equal("~/Views/Home/Index.cshtml", result.ViewName);
    }

    [Fact]
    public void IndexActionModelShopsNotLoadedOnStart()
    {
        var result = Assert.IsType<ViewResult>(_homeController.Index());       

        var indexModel = JsonExtensions.DeserializeAnonymousType(result.Model, new { ShopsUploaded = true });
        Assert.False(indexModel?.ShopsUploaded);
    }    

    #endregion Index

    #region IndexFromQuery

    [Fact]
    public void IndexFromQueryActionReturnsBadRequestWhenShopIdIsNegative()
    {
        var shopId = -1;
        var result = Assert.IsType<BadRequestObjectResult>(_homeController.IndexFromQuery(shopId));
        Assert.Equal($"Invalid shopId : {shopId}", result.Value);
    }    

    [Fact]
    public void IndexFromQueryActionIsIndexViewWhenSuccess()
    {
        var indexView = Assert.IsType<ViewResult>(_homeController.IndexFromQuery(5));
        Assert.Equal("~/Views/Home/Index.cshtml", indexView.ViewName);
    }

    [Fact]
    public void IndexFromQueryModelSelectedShopById()
    {
        var shopId = 5;
        var indexView = Assert.IsType<ViewResult>(_homeController.IndexFromQuery(shopId));

        AssertIndexModel(indexView.Model, shopId);
    }

    #endregion IndexFromQuery

    #region IndexRoute

    [Fact]
    public void IndexRouteActionReturnsBadRequestWhenShopIdIsNegative()
    {
        var shopId = -1;
        var result = Assert.IsType<BadRequestObjectResult>(_homeController.IndexRoute(shopId));
        Assert.Equal($"Invalid shopId : {shopId}", result.Value);
    }    

    [Fact]
    public void IndexRouteActionIsIndexViewWhenSuccess()
    {
        var indexView = Assert.IsType<ViewResult>(_homeController.IndexRoute(3));

        Assert.Equal("~/Views/Home/Index.cshtml", indexView.ViewName);
    }

    [Fact]
    public void IndexRouteyModelSelectedShopById()
    {
        var shopId = 3;

        var indexView = Assert.IsType<ViewResult>(_homeController.IndexRoute(shopId));

        AssertIndexModel(indexView.Model, shopId);
    }

    #endregion IndexRoute

    #region New

    [Fact]
    public void NewActionIsIndexView()
    {
        var indexView = Assert.IsType<ViewResult>(_homeController.New());

        Assert.Equal("~/Views/Home/Index.cshtml", indexView.ViewName);
    }

    [Fact]
    public void NewActionModelShopNotSelected()
    {
        var indexView = Assert.IsType<ViewResult>(_homeController.New());

        AssertIndexModel(indexView.Model, 0);
    }

    #endregion New
}