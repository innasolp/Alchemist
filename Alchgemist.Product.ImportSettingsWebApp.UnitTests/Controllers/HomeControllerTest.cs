using Alchemist.Product.ImportSettingsWebApp.Controllers;
using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace Alchemist.Product.ImportSettingsWebApp.UnitTests.Controllers;

public class HomeControllerTest
{
    [Fact]
    public void Index_ReturnsView()
    {
        var controller = new HomeController(NullLogger<HomeController>.Instance);
        var result = controller.Index();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Privacy_ReturnsView()
    {
        var controller = new HomeController(NullLogger<HomeController>.Instance);
        var result = controller.Privacy();
        Assert.IsType<ViewResult>(result);
    }

    
    [Fact]
    public void ImportSettings_ReturnsIndexView_WithModel()
    {
        var controller = new HomeController(NullLogger<HomeController>.Instance);
        var result = controller.ImportSettings(1, ShopSettingType.Product);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("~/Views/Home/Index.cshtml", view.ViewName);

        var model = Assert.IsType<IndexModel>(view.Model);
        Assert.Equal(1, model.ShopId);
        Assert.Equal(ShopSettingType.Product, model.ShopSettingType);
    }
}
