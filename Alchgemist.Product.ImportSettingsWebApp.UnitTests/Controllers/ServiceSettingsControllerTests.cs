using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.ImportSettingsWebApp.Controllers;
using Alchemist.Product.ImportSettingsWebApp.Infrastructure;
using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.Interfaces;
using Alchgemist.Product.ImportSettingsWebApp.UnitTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Alchemist.Product.ImportSettingsWebApp.UnitTests.Controllers;

public class ServiceSettingsControllerTests : ControllerTest<ServiceSettingsController>
{  
    private readonly Mock<ISettingsDataAdapter> _productSettingsDataAdapterMock = new();
    private readonly Mock<ISettingsDataAdapter> _categorySettingsDataAdapterMock = new();

    protected override ServiceSettingsController CreateController()
    {
        var controller = new ServiceSettingsController(_productSettingsDataAdapterMock.Object, _categorySettingsDataAdapterMock.Object);
        controller.ControllerContext.HttpContext = HttpContextMock.Object;
        return controller;
    }

    private static ProductShopImportSettingsModel NewProductShopSettings(int shopId)
    {
        return new ProductShopImportSettingsModel
        {
            ShopId = shopId,
            Services = new Dictionary<string, ServiceSettingsModel>()
        };
    }    

    [Fact]
    public async Task ServiceSettingsAsync_ReturnsPartialView_WithServiceModel()
    {
        var controller = CreateController();
        var shopSettings = NewProductShopSettings(1);
        var service = new ServiceSettingsModel { Name = "S1", ServiceTypeName = "T1", Guid = Guid.NewGuid() };
        shopSettings.Services.Add(service.Name, service);

        controller.HttpContext.Session.SetImportSettingToSession(shopSettings);

        var result = await controller.ServiceSettingsAsync(1, ShopSettingType.Product, service.Guid);

        var pv = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("~/Views/Home/ServiceSettings.cshtml", pv.ViewName);
        var model = Assert.IsType<ServiceSettingsModel>(pv.Model);
        Assert.Equal("S1", model.Name);
    }

    [Fact]
    public async Task PrimaryServiceAsync_ReturnsBadRequest_WhenNameMismatch()
    {
        var controller = CreateController();

        var result = Assert.IsType< BadRequestObjectResult>(await controller.PrimaryServiceAsync(1, ShopSettingType.Product, "Other"));

        Assert.NotNull(result);
        Assert.Contains("is invalid", result.Value?.ToString());
    }

    [Fact]
    public async Task PrimaryServiceAsync_ReturnsPartialView_WhenNameIsImportService()
    {
        var controller = CreateController();
        var shopSettings = NewProductShopSettings(1);

        controller.HttpContext.Session.SetImportSettingToSession(shopSettings);

        var result = await controller.PrimaryServiceAsync(1, ShopSettingType.Product, nameof(PrimaryServiceName.ImportService));

        var pv = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("~/Views/Home/ServiceSettings.cshtml", pv.ViewName);
    }    

    [Fact]
    public async Task PrimaryServiceAsync_ReturnsPartialView_WhenNameIsBrowserDataLoader()
    {
        var controller = CreateController();

        controller.HttpContext.Session.SetImportSettingToSession(NewProductShopSettings(1));
        
        var result = await controller.PrimaryServiceAsync(1, ShopSettingType.Product, nameof(PrimaryServiceName.BrowserDataLoader));

        var pv = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("~/Views/Home/ServiceSettings.cshtml", pv.ViewName);
    }    

    [Fact]
    public async Task PrimaryServiceAsync_ReturnsPartialView_WhenNameIsBrowserLauncher()
    {
        var controller = CreateController();

        controller.HttpContext.Session.SetImportSettingToSession(NewProductShopSettings(1));
        
        var result = await controller.PrimaryServiceAsync(1, ShopSettingType.Product, nameof(PrimaryServiceName.BrowserLauncher));

        var pv = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("~/Views/Home/ServiceSettings.cshtml", pv.ViewName);
    }    

    [Fact]
    public async Task PrimaryServiceAsync_ReturnsPartialView_WhenNameIsWebLoader()
    {
        var controller = CreateController();

        controller.HttpContext.Session.SetImportSettingToSession(NewProductShopSettings(1));
        
        var result = await controller.PrimaryServiceAsync(1, ShopSettingType.Product, nameof(PrimaryServiceName.WebLoader));

        var pv = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("~/Views/Home/ServiceSettings.cshtml", pv.ViewName);
    }

    [Fact]
    public async Task SaveAsync_ReturnsBadRequest_WhenNameEmpty()
    {
        var controller = CreateController();
        var data = new ServiceSettingsModel { Name = string.Empty };

        var result = Assert.IsType< BadRequestObjectResult>(await controller.SaveAsync(data.ShopId, ShopSettingType.Product, data));

        Assert.NotNull(result);
        Assert.Equal("Empty service name", result.Value);
    }

    [Fact]
    public async Task SaveAsync_ReturnsOk_WhenNoSessionShopSettings()
    {
        var controller = CreateController();
        var data = new ServiceSettingsModel { Name = "S1", ServiceTypeName = "T1", ShopId =1 };

        var result = Assert.IsType<OkObjectResult>(await controller.SaveAsync(1, ShopSettingType.Product, data));

        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task SaveAsync_AddsNewServiceAndReturnsServiceTypeName()
    {
        var shopSettings = NewProductShopSettings(1); 
        
        var controller = CreateController();

        controller.HttpContext.Session.SetImportSettingToSession(shopSettings);

        var data = new ServiceSettingsModel { Name = "S1", ServiceTypeName = "T1", Guid = Guid.NewGuid(), ShopId =1 };

        var result = Assert.IsType<OkObjectResult>(await controller.SaveAsync(1, ShopSettingType.Product, data));

        Assert.NotNull(result);
        Assert.Equal(data, result.Value);

        var updated = await controller.HttpContext.Session.GetShopImportSettingsFromSessionAsync();
        Assert.NotNull(updated);
        Assert.True(updated.Services.ContainsKey("S1"));
    }

    [Fact]
    public async Task IsChanged_ReturnsBadRequest_WhenNameEmpty()
    {
        var controller = CreateController();
        var data = new ServiceSettingsModel { Name = string.Empty };

        var result = Assert.IsType< BadRequestObjectResult>(await controller.IsChanged(data.ShopId, ShopSettingType.Product, data));

        Assert.NotNull(result);
        Assert.Equal("Empty service name", result.Value);
    }

    [Fact]
    public async Task IsChanged_ReturnsOk_WhenNoSessionShopSettings()
    {
        var controller = CreateController();
        var data = new ServiceSettingsModel { Name = "S1", ShopId =1 };

        var result = Assert.IsType<OkObjectResult>(await controller.IsChanged(1, ShopSettingType.Product, data));

        Assert.NotNull(result);
    }

    [Fact]
    public async Task IsChanged_ReturnsFalse_WhenServiceEquals()
    {
        var controller = CreateController();
        var shopSettings = NewProductShopSettings(1);
        var g = Guid.NewGuid();
        var existing = new ServiceSettingsModel { Name = "S1", ServiceTypeName = "T1", Guid = g, ShopId =1 };
        shopSettings.Services.Add(existing.Name, existing);
        
        controller.HttpContext.Session.SetImportSettingToSession(shopSettings);

        var data = new ServiceSettingsModel { Name = "S1", ServiceTypeName = "T1", Guid = g, ShopId =1 };

        var result = await controller.IsChanged(1, ShopSettingType.Product, data) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(false, result.Value);
    }

    [Fact]
    public async Task IsChanged_ReturnsTrue_WhenServiceDifferent()
    {
        var controller = CreateController();
        var shopSettings = NewProductShopSettings(1);
        var existing = new ServiceSettingsModel { Name = "S1", ServiceTypeName = "T1", Guid = Guid.NewGuid() };
        shopSettings.Services.Add(existing.Name, existing);
        
        controller.HttpContext.Session.SetImportSettingToSession(shopSettings);

        var data = new ServiceSettingsModel { Name = "S1", ServiceTypeName = "Different", Guid = existing.Guid, ShopId =1 };

        var result = Assert.IsType<OkObjectResult>(await controller.IsChanged(1, ShopSettingType.Product, data));

        Assert.NotNull(result);
        Assert.Equal(true, result.Value);
    }
}