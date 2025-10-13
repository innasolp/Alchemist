using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.ImportSettingsWebApp.Controllers;
using Alchemist.Product.ImportSettingsWebApp.Infrastructure;
using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.Interfaces;
using Alchgemist.Product.ImportSettingsWebApp.UnitTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Alchgemist.Product.ImportSettingsWebApp.UnitTests.Controllers
{
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

        private static ProductShopImportSettingsModel NewShopSettings(int shopId)
        {
            return new ProductShopImportSettingsModel
            {
                ShopId = shopId,
                Services = []
            };
        }

        [Fact]
        public async Task ServiceSettingsAsync_ReturnsPartialView_WithServiceModel()
        {
            var controller = CreateController();
            var shopSettings = NewShopSettings(1);
            var service = new ServiceSettingsModel { Name = "S1", ServiceTypeName = "T1", Guid = Guid.NewGuid() };
            shopSettings.Services.Add(service.Name, service);

            controller.HttpContext.Session.SetImportSettingtoSession(shopSettings);

            var data = new ServiceSettingsData { ShopId = 1, ShopSettingsType = (int)ShopSettingType.Product, ServiceName = "S1" };

            var result = await controller.ServiceSettingsAsync(data);

            var pv = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("~/Views/Home/ServiceSettings.cshtml", pv.ViewName);
            var model = Assert.IsType<ServiceSettingsModel>(pv.Model);
            Assert.Equal("S1", model.Name);
        }

        [Fact]
        public async Task ImportServiceSettingsAsync_ReturnsBadRequest_WhenNameMismatch()
        {
            var controller = CreateController();
            var data = new ServiceSettingsData { ShopId = 1, ShopSettingsType = (int)ShopSettingType.Product, ServiceName = "Other" };

            var result = await controller.ImportServiceSettingsAsync(data) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Contains(nameof(PrimaryServiceName.ImportService), result.Value?.ToString());
        }

        [Fact]
        public async Task ImportServiceSettingsAsync_ReturnsPartialView_WhenNameMatches()
        {
            var controller = CreateController();
            var shopSettings = NewShopSettings(1);

            controller.HttpContext.Session.SetImportSettingtoSession(shopSettings);

            var data = new ServiceSettingsData { ShopId = 1, ShopSettingsType = (int)ShopSettingType.Product, ServiceName = nameof(PrimaryServiceName.ImportService) };

            var result = await controller.ImportServiceSettingsAsync(data);

            var pv = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("~/Views/Home/ServiceSettings.cshtml", pv.ViewName);
        }

        [Fact]
        public async Task BrowserDataLoaderSettingsAsync_ReturnsBadRequest_WhenNameMismatch()
        {
            var controller = CreateController();
            var data = new ServiceSettingsData { ShopId = 1, ShopSettingsType = (int)ShopSettingType.Product, ServiceName = "Other" };

            var result = await controller.BrowserDataLoaderSettingsAsync(data) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Contains(nameof(PrimaryServiceName.BrowserDataLoader), result.Value?.ToString());
        }

        [Fact]
        public async Task BrowserDataLoaderSettingsAsync_ReturnsPartialView_WhenNameMatches()
        {
            var controller = CreateController();

            controller.HttpContext.Session.SetImportSettingtoSession(NewShopSettings(1));
            var data = new ServiceSettingsData { ShopId = 1, ShopSettingsType = (int)ShopSettingType.Product, ServiceName = nameof(PrimaryServiceName.BrowserDataLoader) };

            var result = await controller.BrowserDataLoaderSettingsAsync(data);

            var pv = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("~/Views/Home/ServiceSettings.cshtml", pv.ViewName);
        }

        [Fact]
        public async Task BrowserLauncherSettingsAsync_ReturnsBadRequest_WhenNameMismatch()
        {
            var controller = CreateController();
            var data = new ServiceSettingsData { ShopId = 1, ShopSettingsType = (int)ShopSettingType.Product, ServiceName = "Other" };

            var result = await controller.BrowserLauncherSettingsAsync(data) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Contains(nameof(PrimaryServiceName.BrowserLauncher), result.Value?.ToString());
        }

        [Fact]
        public async Task BrowserLauncherSettingsAsync_ReturnsPartialView_WhenNameMatches()
        {
            var controller = CreateController();

            controller.HttpContext.Session.SetImportSettingtoSession(NewShopSettings(1));
            var data = new ServiceSettingsData { ShopId = 1, ShopSettingsType = (int)ShopSettingType.Product, ServiceName = nameof(PrimaryServiceName.BrowserLauncher) };

            var result = await controller.BrowserLauncherSettingsAsync(data);

            var pv = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("~/Views/Home/ServiceSettings.cshtml", pv.ViewName);
        }

        [Fact]
        public async Task WebLoaderSettingsAsync_ReturnsBadRequest_WhenNameMismatch()
        {
            var controller = CreateController();
            var data = new ServiceSettingsData { ShopId = 1, ShopSettingsType = (int)ShopSettingType.Product, ServiceName = "Other" };

            var result = await controller.WebLoaderSettingsAsync(data) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Contains(nameof(PrimaryServiceName.WebLoader), result.Value?.ToString());
        }

        [Fact]
        public async Task WebLoaderSettingsAsync_ReturnsPartialView_WhenNameMatches()
        {
            var controller = CreateController();

            controller.HttpContext.Session.SetImportSettingtoSession(NewShopSettings(1));
            
            var data = new ServiceSettingsData { ShopId = 1, ShopSettingsType = (int)ShopSettingType.Product, ServiceName = nameof(PrimaryServiceName.WebLoader) };

            var result = await controller.WebLoaderSettingsAsync(data);

            var pv = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("~/Views/Home/ServiceSettings.cshtml", pv.ViewName);
        }

        [Fact]
        public async Task SaveAsync_ReturnsBadRequest_WhenNameEmpty()
        {
            var controller = CreateController();
            var data = new ServiceSettingsModel { Name = string.Empty };

            var result = await controller.SaveAsync(data) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal("Empty service name", result.Value);
        }

        [Fact]
        public async Task SaveAsync_ReturnsOk_WhenNoSessionShopSettings()
        {
            var controller = CreateController();
            // session empty -> GetShopImportSettingsFromSessionAsync should return null
            var data = new ServiceSettingsModel { Name = "S1", ServiceTypeName = "T1" };

            var result = await controller.SaveAsync(data) as OkResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SaveAsync_AddsNewServiceAndReturnsServiceTypeName()
        {
            var shopSettings = NewShopSettings(1);            
            
            var controller = CreateController();

            controller.HttpContext.Session.SetImportSettingtoSession(shopSettings);

            var data = new ServiceSettingsModel { Name = "S1", ServiceTypeName = "T1", Guid = Guid.NewGuid() };

            var result = await controller.SaveAsync(data) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal("T1", result.Value);

            var updated = await controller.HttpContext.Session.GetShopImportSettingsFromSessionAsync();
            Assert.NotNull(updated);
            Assert.True(updated.Services.ContainsKey("S1"));
        }

        [Fact]
        public async Task IsChanged_ReturnsBadRequest_WhenNameEmpty()
        {
            var controller = CreateController();
            var data = new ServiceSettingsModel { Name = string.Empty };

            var result = await controller.IsChanged(data) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal("Empty service name", result.Value);
        }

        [Fact]
        public async Task IsChanged_ReturnsOk_WhenNoSessionShopSettings()
        {
            var controller = CreateController();
            var data = new ServiceSettingsModel { Name = "S1" };

            var result = await controller.IsChanged(data) as OkObjectResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task IsChanged_ReturnsFalse_WhenServiceEquals()
        {
            var controller = CreateController();
            var shopSettings = NewShopSettings(1);
            var g = Guid.NewGuid();
            var existing = new ServiceSettingsModel { Name = "S1", ServiceTypeName = "T1", Guid = g };
            shopSettings.Services.Add(existing.Name, existing);
            
            controller.HttpContext.Session.SetImportSettingtoSession(shopSettings);

            var data = new ServiceSettingsModel { Name = "S1", ServiceTypeName = "T1", Guid = g };

            var result = await controller.IsChanged(data) as OkObjectResult;
            Assert.NotNull(result);
            Assert.Equal(false, result.Value);
        }

        [Fact]
        public async Task IsChanged_ReturnsTrue_WhenServiceDifferent()
        {
            var controller = CreateController();
            var shopSettings = NewShopSettings(1);
            var existing = new ServiceSettingsModel { Name = "S1", ServiceTypeName = "T1", Guid = Guid.NewGuid() };
            shopSettings.Services.Add(existing.Name, existing);
           
            controller.HttpContext.Session.SetImportSettingtoSession(shopSettings);

            var data = new ServiceSettingsModel { Name = "S1", ServiceTypeName = "Different", Guid = existing.Guid };

            var result = await controller.IsChanged(data) as OkObjectResult;
            Assert.NotNull(result);
            Assert.Equal(true, result.Value);
        }
    }
}