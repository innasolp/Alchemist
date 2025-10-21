using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.ImportSettingsWebApp.Controllers;
using Alchemist.Product.ImportSettingsWebApp.Infrastructure;
using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.Interfaces;
using Alchgemist.Product.ImportSettingsWebApp.UnitTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Alchgemist.Product.ImportSettingsWebApp.UnitTests.Controllers;

public class ImportSettingsControllerTest : ControllerTest<ImportSettingsController>
{
    private readonly Mock<ISettingsDataAdapter> _productAdapter = new();

    private readonly Mock<ISettingsDataAdapter> _categoryAdapter = new();

    protected override ImportSettingsController CreateController()
    {
        var controller = new ImportSettingsController(_productAdapter.Object, _categoryAdapter.Object);
        controller.ControllerContext.HttpContext = HttpContextMock.Object;
        return controller;
    }    

    private static ProductShopImportSettingsModel NewProductSettings(int shopId, string name = "Shop") => new()
    {
        ShopId = shopId,
        Name = name,
        Perfomance = true,
        ProductUrlFormat = "p/{id}",
        CategoryUrlFormat = "c/{id}",
        PageProductCount = 20,
    };

    private static CategoryShopImportSettingsModel NewCategorySettings(int shopId, string name = "Shop") => new()
    {
        ShopId = shopId,
        Name = name,
        Perfomance = true,
        CategorySourceUrl = "https://example.com/categories"
    };

    private static ServiceSettingsModel NewService(string key, string name = "Service") => new()
    {
        Name = name,
        ServiceTypeName = "Type",
        ShopId = 1,
        JsonValue = null
    };

    [Fact]
    public async Task ImportSettingsTabAsync_ReturnsPartialView_AndSetsSession_ForProduct()
    {
        var product = NewProductSettings(1);
        _productAdapter
            .Setup(a => a.GetShopImportSettings(1))
            .ReturnsAsync(product);

        var controller = CreateController();

        var result = await controller.ImportSettingsTabAsync(new ShopSettingsData { ShopId = 1, ShopSettingsType = (int)ShopSettingType.Product });

        var pv = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("~/Views/Shared/ShopImportSettingsTab.cshtml", pv.ViewName);
        var model = Assert.IsAssignableFrom<ShopImportSettingsModel>(pv.Model);
        Assert.Equal(1, model.ShopId);
        Assert.Equal(ShopSettingType.Product, model.ShopSettingType);

        // Session should contain stored settings
        Assert.True(controller.HttpContext.Session.TryGetValue(SessionKeys.ShopImportSettingsKey, out _));
        Assert.Equal((int)ShopSettingType.Product, controller.HttpContext.Session.GetInt32(SessionKeys.ShopSettingsTypeKey));

        _productAdapter.Verify(a => a.GetShopImportSettings(1), Times.Once);
        _categoryAdapter.VerifyNoOtherCalls();
    }

    [Fact]
    public void LoadProductShopImportSettings_BadRequest_WhenNull()
    {
        var controller = CreateController();

        var result = controller.LoadProductShopImportSettings(null);

        var br = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Empty json", br.Value?.ToString());
    }

    [Fact]
    public void LoadProductShopImportSettings_ReturnsPartialView_AndSetsSession()
    {
        var controller = CreateController();
        var data = NewProductSettings(5);

        var result = controller.LoadProductShopImportSettings(data);

        var pv = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("~/Views/Shared/ImportSettings.cshtml", pv.ViewName);
        Assert.Same(data, pv.Model);
        Assert.True(controller.HttpContext.Session.TryGetValue(SessionKeys.ShopImportSettingsKey, out _));
    }

    [Fact]
    public async Task ProductSettingsIsChangedAsync_BadRequest_WhenNull()
    {
        var controller = CreateController();
        var result = await controller.ProductSettingsIsChangedAsync(null);
        var br = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Empty json", br.Value?.ToString());
    }

    [Fact]
    public async Task ProductSettingsIsChangedAsync_OkTrue_WhenNoExistingSettings()
    {        
        _productAdapter.Setup(a => a.GetShopImportSettings(10)).ReturnsAsync((IShopImportSettings?)null);
        var controller = CreateController();

        var data = NewProductSettings(10);
        var result = await controller.ProductSettingsIsChangedAsync(data);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(true, ok.Value);
        _productAdapter.Verify(a => a.GetShopImportSettings(10), Times.Once);
    }

    
    [Fact]
    public async Task ProductSettingsIsChangedAsync_OkTrue_WhenSessionServicesDiffer()
    {
        var controller = CreateController();

        var existing = NewProductSettings(3);
        existing.Services["ImportService"] = NewService("ImportService", name: "ImportService");
        _productAdapter.Setup(a => a.GetShopImportSettings(3)).ReturnsAsync(existing);

        var sessionModel = NewProductSettings(3);
        sessionModel.Services["ImportService"] = NewService("ImportService", name: "DifferentServiceName");
        controller.HttpContext.Session.SetImportSettingToSession(sessionModel);
        
        var data = NewProductSettings(3);
        var result = await controller.ProductSettingsIsChangedAsync(data);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(true, ok.Value);
    }

    [Fact]
    public async Task ProductSettingsSave_BadRequest_WhenNull()
    {
        var controller = CreateController();
        var result = await controller.ProductSettingsSave(null);
        var br = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Empty json", br.Value?.ToString());
    }

    [Fact]
    public async Task ProductSettingsSave_SavesUpdatedExisting_AndReturnsOk()
    {
        var controller = CreateController();

        var existing = NewProductSettings(4, name: "Old");
        existing.ProductUrlFormat = "old-p";
        existing.CategoryUrlFormat = "old-c";
        existing.PageProductCount = 5;

        // existing has no services to start
        _productAdapter.Setup(a => a.GetShopImportSettings(4)).ReturnsAsync(existing);

        // Capture the saved object to assert on it
        ProductShopImportSettingsModel? saved = null;
        _productAdapter
            .Setup(a => a.Save(It.IsAny<IShopImportSettings>()))
            .Callback<IShopImportSettings>(m => saved = Assert.IsType<ProductShopImportSettingsModel>(m))
            .Returns(Task.CompletedTask);

        // session contains one service, which should be applied to existing when saving
        var sessionModel = NewProductSettings(4);
        sessionModel.Services["WebLoader"] = NewService("WebLoader", name: "WebLoader");
        controller.HttpContext.Session.SetImportSettingToSession(sessionModel);
        

        var payload = NewProductSettings(4, name: "New");
        payload.ProductUrlFormat = "new-p";
        payload.CategoryUrlFormat = "new-c";
        payload.PageProductCount = 10;

        var result = await controller.ProductSettingsSave(payload);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(true, ok.Value);

        _productAdapter.Verify(a => a.GetShopImportSettings(4), Times.Once);
        _productAdapter.Verify(a => a.Save(It.IsAny<IShopImportSettings>()), Times.Once);
        _categoryAdapter.VerifyNoOtherCalls();

        Assert.NotNull(saved);
        Assert.Equal("New", saved!.Name);
        Assert.Equal("new-p", saved.ProductUrlFormat);
        Assert.Equal("new-c", saved.CategoryUrlFormat);
        Assert.Equal(10, saved.PageProductCount);
        Assert.True(saved.Services.ContainsKey("WebLoader"));
        Assert.Equal("WebLoader", saved.Services["WebLoader"].Name);
    }

    [Fact]
    public void LoadCategoryShopImportSettings_BadRequest_WhenNull()
    {
        var controller = CreateController();
        var result = controller.LoadCategoryShopImportSettings(null);
        var br = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Empty json", br.Value?.ToString());
    }

    [Fact]
    public void RootCategory_ReturnsPartialView_WhenValid()
    {
        var controller = CreateController();

        var data = new CategoryUrlModel() { Item = 1, Url = "https://example.com/c/1", ShopId = 1 };

        var result = controller.RootCategory(data);

        var pv = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("~/Views/Home/RootCategoryUrl.cshtml", pv.ViewName);
        Assert.Same(data, pv.Model);
    }

    [Fact]
    public async Task SetRootCategory_AddsNew_WhenNotExist()
    {
        var product = NewProductSettings(1);
        product.RootCategories = new List<CategoryUrlModel>();

        _productAdapter.Setup(a => a.GetShopImportSettings(1)).ReturnsAsync(product);

        var controller = CreateController();

        var data = new CategoryUrlModel() { Item = 5, Url = "https://example.com/c/5", ShopId = 1 };

        var result = await controller.SetRootCategory(data) as OkObjectResult;

        Assert.NotNull(result);
        var returned = Assert.IsType<CategoryUrlModel>(result.Value);
        Assert.Equal(data.Url, returned.Url);

        var stored = await controller.HttpContext.Session.GetShopImportSettingsFromSessionAsync() as ProductShopImportSettingsModel;
        Assert.NotNull(stored);
        Assert.Contains(stored.RootCategories, c => c.Url == data.Url && c.Item == data.Item);
    }

    [Fact]
    public async Task SetRootCategory_UpdatesExisting_WhenExists()
    {
        var existing = new CategoryUrlModel() { Item = 7, Url = "https://old", ShopId = 1, Guid = Guid.NewGuid() };
        var product = NewProductSettings(1);
        product.RootCategories = new List<CategoryUrlModel> { existing };

        _productAdapter.Setup(a => a.GetShopImportSettings(1)).ReturnsAsync(product);

        var controller = CreateController();

        var data = new CategoryUrlModel() { Item = 7, Url = "https://new", ShopId = 1, Guid = existing.Guid };

        var result = await controller.SetRootCategory(data) as OkObjectResult;

        Assert.NotNull(result);
        var returned = Assert.IsType<CategoryUrlModel>(result.Value);
        Assert.Equal("https://new", returned.Url);

        var stored = await controller.HttpContext.Session.GetShopImportSettingsFromSessionAsync() as ProductShopImportSettingsModel;
        Assert.NotNull(stored);
        Assert.Contains(stored.RootCategories, c => c.Url == "https://new" && c.Item == 7);
    }

    [Fact]
    public async Task RootCategoryIsChanged_ReturnsTrue_WhenNotExists()
    {
        var product = NewProductSettings(2);
        product.RootCategories = new List<CategoryUrlModel>();

        _productAdapter.Setup(a => a.GetShopImportSettings(2)).ReturnsAsync(product);

        var controller = CreateController();

        var data = new CategoryUrlModel() { Item = 10, Url = "https://example.com/c/10", ShopId = 2 };

        var result = await controller.RootCategoryIsChanged(data) as OkObjectResult;
        Assert.NotNull(result);
        Assert.Equal(true, result.Value);
    }

    [Fact]
    public async Task RootCategoryIsChanged_ReturnsFalse_WhenEquals()
    {
        var existing = new CategoryUrlModel() { Item = 3, Url = "https://same", ShopId = 3 };
        var product = NewProductSettings(3);
        product.RootCategories = new List<CategoryUrlModel> { existing };

        _productAdapter.Setup(a => a.GetShopImportSettings(3)).ReturnsAsync(product);

        var controller = CreateController();

        var data = new CategoryUrlModel() { Item = 3, Url = "https://same", ShopId = 3 };

        var result = await controller.RootCategoryIsChanged(data) as OkObjectResult;
        Assert.NotNull(result);
        Assert.Equal(false, result.Value);
    }

    [Fact]
    public void RootCategoryItem_ReturnsPartialView()
    {
        var controller = CreateController();

        var data = new CategoryUrlModel() { Item = 2, Url = "u", ShopId = 1 };

        var result = controller.RootCategoryItem(data);

        var pv = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("~/Views/Home/RootCategoryUrlItem.cshtml", pv.ViewName);
        Assert.Same(data, pv.Model);
    }
}
