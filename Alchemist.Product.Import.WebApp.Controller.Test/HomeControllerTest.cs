using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Controllers;
using Alchemist.Product.Import.WebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.Import.WebApp.Controller.Test;

public class HomeControllerTest : ControllerTest<HomeController>
{
    private async Task<ViewResult> IndexActionIsTypeViewResultAsync()
    {
        var homeController = CreateHomeController();
        var actionResult = await homeController.Index();
        Assert.NotNull(actionResult);
        return Assert.IsType<ViewResult>(actionResult);
    }

    [Fact]
    public async Task IndexActionModelIsIndexViewModelAsync()
    {
        var actionResult = await IndexActionIsTypeViewResultAsync();
        Assert.IsType<IndexViewModel>(actionResult.Model);
    }

    [Fact]
    public async Task ViewModelIsEmptyOnFirstIndexActionAsync()
    {
        var view = await IndexActionIsTypeViewResultAsync();
        var indexViewModel = Assert.IsType<IndexViewModel>(view.Model);
        Assert.Empty(indexViewModel.Shops);
    }   

    [Fact]
    public async Task ShopListNotEmptyAfterUpdateShopsActionAndIndexActionAsync()
    {
        var homeController = CreateHomeController();
        var actionResult = Assert.IsType<OkObjectResult>(await homeController.UpdateShops());
        Assert.Equal(_shops.Count, Assert.IsType<List<ShopModel>>(actionResult.Value).Count);
    }

    [Fact]
    public async Task UpdateShopsActionResultIsInternalServerErrorWhenLoadShopsThrowExceptionAsync()
    {
        var exception = new InvalidOperationException("test error");
        var homeController = CreateHomeController();
        _shopDataServiceMock.Setup(s => s.GetShops()).Throws(exception);
        
        var actionResult = Assert.IsType<ObjectResult>(await homeController.UpdateShops());
        Assert.Equal(exception.Message, Assert.IsType<InvalidOperationException>(actionResult.Value).Message);        
    }    

    [Fact]
    public async Task IndexActionWhenSelectedShopChangedAsync()
    {
        var homeController = CreateHomeController();

        await SetShopsAsync();

        var nextShopGuid = _importFacade.GetShops().Last().ShopGuid;
        var selectedTab = TabType.Shop;

        var nextIndexView = Assert.IsType<ViewResult>(await homeController.IndexFromQueryAsync(nextShopGuid, (int)selectedTab));
        var nextIndexViewModel = Assert.IsType<IndexViewModel>(nextIndexView.Model);
        Assert.Equal(selectedTab, nextIndexViewModel.SelectedTab);
        Assert.Equal(nextShopGuid, nextIndexViewModel.SelectedShopImport.ShopGuid);
    }

    [Fact]
    public async Task IndexActionWhenSelectedTabChangedAsync()
    {
        var homeController = CreateHomeController(); 

        await SetShopsAsync();

        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var nextTab = TabType.Products;

        var nextIndexView = Assert.IsType<ViewResult>(await homeController.IndexFromQueryAsync(shopGuid, (int)nextTab));
        var nextIndexViewModel = Assert.IsType<IndexViewModel>(nextIndexView.Model);        
        Assert.Equal(nextTab, nextIndexViewModel.SelectedTab);
        Assert.Equal(shopGuid, nextIndexViewModel.SelectedShopImport.ShopGuid);
    }


    [Fact]
    public async Task IndexActionNotFoundWhenNonexistingShopSetAsync()
    {
        await SetShopsAsync();

        var homeController = CreateHomeController();        

        var tab = TabType.Shop;
        var nextShopGuid = Guid.NewGuid();

        Assert.Equal(nextShopGuid,
            Assert.IsType<Guid>(Assert.IsType<NotFoundObjectResult>(await homeController.IndexFromQueryAsync(nextShopGuid, (int)tab)).Value));
    }

    [Fact]
    public async Task IndexActionBadRequestWhenNonexistentTabSetAsync()
    {
        var homeController = CreateHomeController();
        await SetShopsAsync();

        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var nextTab = (int)Enum.GetValues<TabType>().Max() + 1;
        Assert.IsType<BadRequestResult>(await homeController.IndexFromQueryAsync(shopGuid, nextTab));
    }

    [Fact]
    public async Task IndexActionBadRequestWhenEmptyShopGuidSetAsync()
    {
        var homeController = CreateHomeController();
        await SetShopsAsync();
        Assert.IsType<BadRequestResult>(await homeController.IndexFromQueryAsync(Guid.Empty, (int)TabType.Shop));
    }
}
