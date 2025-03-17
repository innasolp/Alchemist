using Alchemist.DataService.Interfaces;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.WebApp.Controllers;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Alchemist.Product.Import.WebApp.Controller.Test;

public class ShopControllerTest : ControllerTest<ShopController>
{  
    private ShopController CreateShopController()
    {
        return new ShopController(_importFacade, _shopDataServiceMock.Object);
    }

    [Fact]
    public void NewActionIsPartialView()
    {
        var shopController = CreateShopController();
        Assert.IsType<PartialViewResult>(shopController.New());
    }

    [Fact]
    public void NewActionModelIsShopModel()
    {
        var shopController = CreateShopController();
        var viewResult = Assert.IsType<PartialViewResult>(shopController.New());
        Assert.IsType<ShopModel>(viewResult.Model);
    }

    [Fact]
    public async Task SaveActionIsBadRequestWhenShopIsNullAsync()
    {
        var shopController = CreateShopController();
        Assert.IsType<BadRequestObjectResult>(await shopController.Save(null));
    }

    [Fact]
    public async Task SaveActionIsNotFoundWhenShopWithNonexistsGuidAsync()
    {
        var homeController = CreateHomeController();
        Assert.IsType<OkObjectResult>(await homeController.UpdateShops());

        var shopController = CreateShopController();
        var shop = new ShopModel { Id = _importFacade.GetShops().First().Shop.Id };
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(await shopController.Save(shop));
        var notFoundShop = Assert.IsType<ShopModel>(notFoundResult.Value);
        Assert.Equal(shop.Id, notFoundShop.Id);
        Assert.Equal(shop.Guid, notFoundShop.Guid);
    }

    [Fact]
    public async Task SaveActionInternalServerErrorResultWhenShopDataServiceExceptionThrowsAsync()
    {
        var shopController = CreateShopController();
        var shop = new ShopModel();
        _shopDataServiceMock.Setup(s => s.CreateShop(It.IsAny<IShop>())).
            Returns(CreateShopThrowsExceptionAsync);

        var internalServerErrorResult = Assert.IsType<ObjectResult>(await shopController.Save(shop));
        Assert.Equal(StatusCodes.Status500InternalServerError, internalServerErrorResult.StatusCode);       
    }

    private async Task<IShop> CreateShopThrowsExceptionAsync(IShop shop)
    {
        throw new InvalidOperationException(nameof(SaveActionInternalServerErrorResultWhenShopDataServiceExceptionThrowsAsync));
    }

    [Fact]
    public async Task SaveActionIsOkShopGuidWhenNewShopAsync()
    {
        var homeController = CreateHomeController();
        Assert.IsType<OkObjectResult>(await homeController.UpdateShops());

        var shopController = CreateShopController();
        var shop = new ShopModel() { Name = nameof(SaveActionIsOkShopGuidWhenNewShopAsync), Url="https://test" };
        _shopDataServiceMock.Setup(s => s.CreateShop(It.IsAny<IShop>())).
            Returns(CreateShopAsync);
        var prevShopCount = _importFacade.GetShops().Count;

        var okResult = Assert.IsType<OkObjectResult>(await shopController.Save(shop));
        Assert.IsType<Guid>(okResult.Value);       
        Assert.Equal(prevShopCount + 1, _importFacade.GetShops().Count);
        Assert.Contains(_importFacade.GetShops(), s => s.Shop.Name == shop.Name);
    }

    private async Task<IShop> CreateShopAsync(IShop newShop)
    {
        _shops.Add(newShop);
        return await Task.FromResult(newShop);
    }
    [Fact]
    public async Task SaveActionIsOkShopGuidWhenShopUpdatedAsync()
    {
        var homeController = CreateHomeController();
        Assert.IsType<OkObjectResult>(await homeController.UpdateShops());

        var shopController = CreateShopController();
        var index = new Random().Next(_shops.Count);
        var shop = _importFacade.GetShops()[index].Shop;
        _shopDataServiceMock.Setup(s => s.UpdateShop(It.IsAny<IShop>())).
            Returns(UpdateShopAsync);
        var prevShopCount = _importFacade.GetShops().Count;

        var okResult = Assert.IsType<OkObjectResult>(await shopController.Save(shop));
        Assert.Equal(shop.Guid, Assert.IsType<Guid>(okResult.Value));
        Assert.Equal(prevShopCount, _importFacade.GetShops().Count);
        Assert.Contains(_importFacade.GetShops(), s => s.Shop.Name == shop.Name);
    }

    private async Task<IShop> UpdateShopAsync(IShop shop)
    {
        var existingShop = _shops.FirstOrDefault(s => s.Id == shop.Id);
        existingShop.Name = Guid.NewGuid().ToString();
        return await Task.FromResult(existingShop);
    }
}
