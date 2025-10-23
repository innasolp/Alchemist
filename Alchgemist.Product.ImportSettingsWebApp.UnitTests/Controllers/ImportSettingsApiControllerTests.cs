using Alchemist.Product.ImportSettingsWebApp.Controllers;
using Alchgemist.Product.ImportSettingsWebApp.UnitTests.Infrastructure;
using Moq;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Product.ImportSettingsWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Alchemist.Product.Interfaces;
using Alchemist.Product.ImportSettingsWebApp.Infrastructure;


namespace Alchemist.Product.ImportSettingsWebApp.UnitTests.Controllers;

public class ImportSettingsApiControllerTests : ControllerTest<ImportSettingsApiController>
{
    private readonly Mock<ISettingsDataAdapter> _productAdapter = new();
    private readonly Mock<ISettingsDataAdapter> _categoryAdapter = new();

    protected override ImportSettingsApiController CreateController()
    {
        var controller = new ImportSettingsApiController(_productAdapter.Object, _categoryAdapter.Object);
        controller.ControllerContext.HttpContext = HttpContextMock.Object;
        return controller;
    }

    [Fact]
    public async Task ShopSettingsTab_ReturnsPartialView_AndSetsSession()
    {
        var product = new ProductShopImportSettingsModel { ShopId = 1 };
        _productAdapter.Setup(a => a.GetShopImportSettings(1)).ReturnsAsync(product);

        var controller = CreateController();

        var result = await controller.ShopSettingsTab(1, ShopSettingType.Product);

        var pv = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("~/Views/Shared/ShopImportSettingsTab.cshtml", pv.ViewName);
        var model = Assert.IsAssignableFrom<ShopImportSettingsModel>(pv.Model);
        Assert.Equal(1, model.ShopId);
        Assert.Equal(ShopSettingType.Product, model.ShopSettingType);

        // Session should contain stored settings
        Assert.NotNull(await controller.HttpContext.Session.GetShopImportSettingsFromSessionAsync());
    }
}
