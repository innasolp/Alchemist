using Alchemist.Product.ImportSettingsWebApp.Controllers;
using Alchgemist.Product.ImportSettingsWebApp.UnitTests.Infrastructure;
using Moq;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Product.ImportSettingsWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Alchemist.Product.Interfaces;


namespace Alchemist.Product.ImportSettingsWebApp.UnitTests.Controllers;

public class ImportSettingsApiControllerTests : ControllerTest<ImportSettingsApiController>
{
    private readonly Mock<ISettingsDataAdapter> _productAdapter = new();
    private readonly Mock<ISettingsDataAdapter> _categoryAdapter = new();

    protected override ImportSettingsApiController CreateController()
    {
        var controller = new ImportSettingsApiController();
        controller.ControllerContext.HttpContext = HttpContextMock.Object;
        return controller;
    }

    [Fact]
    public void ShopSettings_ReturnsPartialView()
    {
        var controller = CreateController();

        var result = controller.ShopImportSettings(1, ShopSettingType.Product);

        var pv = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("~/Views/Shared/ShopImportSettingsTab.cshtml", pv.ViewName);
        var model = Assert.IsAssignableFrom<IndexModel>(pv.Model);
        Assert.Equal(1, model.ShopId);
        Assert.Equal(ShopSettingType.Product, model.ShopSettingType);
    }

    [Fact]
    public void Default_ReturnsPartialView_WithEmptyIndexModel()
    {
        var controller = CreateController();

        var result = controller.Default();

        var pv = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("~/Views/Shared/ShopImportSettingsTab.cshtml", pv.ViewName);
        var model = Assert.IsAssignableFrom<IndexModel>(pv.Model);
        Assert.Null(model.ShopId);
        Assert.Equal(ShopSettingType.Product, model.ShopSettingType);
    }

    [Fact]
    public void ShopSettings_ReturnsBadRequest_WhenShopIdIsNegative()
    {
        var controller = CreateController();
        var shopId = -1;
        var result = controller.ShopImportSettings(shopId, ShopSettingType.Product);

        var pv = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal($"shopId {shopId} is invalid.", pv.Value);        
    }

    [Fact]
    public void ShopSettings_ReturnsBadRequest_WhenShopSettingsTypeIsService()
    {
        var controller = CreateController();
        var shopSettingsType = ShopSettingType.Service;
        var result = controller.ShopImportSettings(5, shopSettingsType);

        var pv = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal($"shopSettingsType {shopSettingsType} is invalid.", pv.Value);        
    }
}
