using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Controllers;
using Alchemist.Product.Import.WebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Data;
using System.Text.Json;
using ShopSettingType = Alchemist.Import.Settings.Interfaces.ShopSettingType;

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
    public async Task UpdateShopsActionResultIsOkShopModelList()
    {
        var homeController = CreateHomeController();

        var actionResult = Assert.IsType<OkObjectResult>(await homeController.UpdateShops());
        Assert.IsType<List<ShopModel>>(actionResult.Value);       
    }

    [Fact]
    public async Task UpdateShopsActionResultIsLoadedShopModels()
    {
        await SetShopsAsync();

        var shops = _importFacade.GetShops().Select(s=>s.Shop).ToList();

        var homeController = CreateHomeController();
        
        var actionResult = Assert.IsType<OkObjectResult>(await homeController.UpdateShops());
        var model = Assert.IsType<List<ShopModel>>(actionResult.Value);
        Assert.True(model.All(s => shops.Any(s1 => s1.Guid == s.Guid)));
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

    [Fact]
    public async Task IsTabChangedBadRequestWhenJsonIsEmpty()
    {
        var homeController = CreateHomeController();
        var result = Assert.IsType<BadRequestObjectResult>(await homeController.IsTabChanged(Guid.NewGuid(), 1, string.Empty));
        Assert.Equal(string.Empty, Assert.IsType<string>(result.Value));
    }

    [Fact]
    public async Task IsTabChangedNotFoundWhenNonExistingShopGuid()
    {
        await SetShopsAsync();
        var homeController = CreateHomeController();
        var guid = Guid.NewGuid();
        string json = Guid.NewGuid().ToString();
        var result = Assert.IsType<NotFoundObjectResult>(await homeController.IsTabChanged(guid, 1, json));
        Assert.Equal(guid, Assert.IsType<Guid>(result.Value));
    }

    [Fact]
    public async Task IsTabChangedOkFalseWhenNonExistingTabSetting()
    {
        await SetShopsAsync();
        var homeController = CreateHomeController();
        var guid = _importFacade.GetShops().First().ShopGuid;
        string json = Guid.NewGuid().ToString();

        var result = Assert.IsType<OkObjectResult>(await homeController.IsTabChanged(guid, 1, json));
        Assert.False(Assert.IsType<bool>(result.Value));
    }

    [Fact]
    public async Task IsTabChangedBadRequestWhenInvalidJson()
    {
        await SetShopsAsync();
        var homeController = CreateHomeController();
        var guid = _importFacade.GetShops().First().ShopGuid;
        var shopSettingsType = ShopSettingType.Product;
        string json = Guid.NewGuid().ToString();

        await SetShopSettingAsync(guid, shopSettingsType);

        var result = Assert.IsType<BadRequestObjectResult>(await homeController.IsTabChanged(guid, (int)TabType.Shop, json));
        Assert.Equal(json, Assert.IsType<string>(result.Value));
    }

    [Fact]
    public async Task IsTabChangedOkFalseWhenJsonEqualShopSettings()
    {
        await SetShopsAsync();
        var homeController = CreateHomeController();

        var guid = _importFacade.GetShops().First().ShopGuid;
        var shopSettingsType = ShopSettingType.Product;       
        await SetShopSettingAsync(guid, shopSettingsType);

        Assert.True(_importFacade.TryGetShopSettings(guid, shopSettingsType, out var shopSettings));
        var json = JsonSerializer.Serialize(shopSettings, shopSettings.GetType(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var result = Assert.IsType<OkObjectResult>(await homeController.IsTabChanged(guid, (int)TabType.Shop, json));
        Assert.False(Assert.IsType<bool>(result.Value));
    }

    [Fact]
    public async Task ShopListActionResultViewNameIs_ShopListPartial()
    {
        var homeController = CreateHomeController();
        var view = Assert.IsType<PartialViewResult>(homeController.ShopList([]));
        Assert.Equal("~/Views/Home/_ShopListPartial.cshtml", view.ViewName);
    }

    [Fact]
    public async Task ShopListActionResultModelIsShopModelEnumerable()
    {
        var homeController = CreateHomeController();
        var view = Assert.IsType<PartialViewResult>(homeController.ShopList([]));
        Assert.IsAssignableFrom<IEnumerable<ShopModel>>(view.Model);
    }

    [Fact]
    public async Task TabsMenuActionResultViewNameIs_TabsMenuPartial()
    {
        var homeController = CreateHomeController();
        var view = Assert.IsType<PartialViewResult>(homeController.TabsMenu(Guid.NewGuid(), (int)TabType.Shop));
        Assert.Equal("~/Views/Home/_TabsMenuPartial.cshtml", view.ViewName);
    }

    [Fact]
    public async Task TabsMenuActionResultModelIsDefaultTabModelWhenNoShops()
    {
        var homeController = CreateHomeController();
        var view = Assert.IsType< PartialViewResult>(homeController.TabsMenu(Guid.NewGuid(), (int)TabType.Shop));
        var model = Assert.IsType<ShopSettingTabsModel>(view.Model);
        Assert.Null(model.ShopProductsSettings);
        Assert.Null(model.ShopCategoriesSettings);
    }

    [Fact]
    public async Task TabsMenuActionResultModelIsFirstShopSettingsWhenEmptyShopGuid()
    {
        await SetShopsAsync();

        var shopImport = _importFacade.GetShops().First();
        await SetShopSettingAsync(shopImport.ShopGuid, ShopSettingType.Product);

        var homeController = CreateHomeController();
        var view = Assert.IsType<PartialViewResult>(homeController.TabsMenu(Guid.Empty, (int)TabType.Shop));
        var model = Assert.IsType<ShopSettingTabsModel>(view.Model);

        Assert.NotNull(model.ShopProductsSettings);
        Assert.Equal(shopImport.ShopSettingTabs.Guid, model.Guid);
    }

    [Fact]
    public async Task TabsMenuActionResultModelIsDefaultTabModelWhenNotExistingShopGuid()
    {
        await SetShopsAsync();
        var homeController = CreateHomeController();
        var view = Assert.IsType<PartialViewResult>(homeController.TabsMenu(Guid.NewGuid(), (int)TabType.Shop));
        var model = Assert.IsType<ShopSettingTabsModel>(view.Model);
        Assert.Null(model.ShopProductsSettings);
        Assert.Null(model.ShopCategoriesSettings);
    }

    [Fact]
    public async Task TabsMenuActionResultModelIsTabModelShopGuid()
    {
        await SetShopsAsync();

        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var productShopSettings = await SetShopSettingAsync(shopGuid, ShopSettingType.Product);
        var categoryShopSettings = await SetShopSettingAsync(shopGuid, ShopSettingType.Category);

        var homeController = CreateHomeController();
        var view = Assert.IsType<PartialViewResult>(homeController.TabsMenu(shopGuid, (int)TabType.Shop));
        var model = Assert.IsType<ShopSettingTabsModel>(view.Model);
        Assert.True(model.ShopProductsSettings?.Equals(productShopSettings));
        Assert.True(model.ShopCategoriesSettings?.Equals(categoryShopSettings));
    }

    [Fact]
    public async Task LoadTabActionResultViewNameIsShopSettingTabsWhenTabTypeIsShop()
    {
        await LoadTabActionResultViewNameAsync(TabType.Shop, "ShopSettingTabs.cshtml");
    }

    [Fact]
    public async Task LoadTabActionResultViewNameIsImportProductsTabsWhenTabTypeIsProducts()
    {
        await LoadTabActionResultViewNameAsync(TabType.Products, "ImportProducts.cshtml");

    }

    [Fact]
    public async Task LoadTabActionResultViewNameIsImportCategoriesTabsWhenTabTypeIsCategories()
    {
        await LoadTabActionResultViewNameAsync(TabType.Products, "ImportCategories.cshtml");
    }

    private async Task LoadTabActionResultViewNameAsync(TabType tabType, string viewName)
    {
        var homeController = CreateHomeController();
        var view = Assert.IsType<PartialViewResult>(await homeController.LoadTab(Guid.NewGuid(), (int)tabType, viewName));
        Assert.Equal(view.ViewName, $"~/Views/Home/{viewName}.cshtml");
    }

    [Fact]
    public async Task LoadTabActionResultModelIsDefaultTabModelWhenNotExistingShopGuid()
    {
        await SetShopsAsync();
        var homeController = CreateHomeController();
        var tabType = TabType.Shop;
        var tabView = TabHelper.TabViewNames.FirstOrDefault(d => d.Key == tabType).Value;
        var view = Assert.IsType<PartialViewResult>(await homeController.LoadTab(Guid.NewGuid(), (int)tabType, tabView));
        var model = Assert.IsType<ShopSettingTabsModel>(view.Model);
        Assert.Null(model.ShopProductsSettings);
        Assert.Null(model.ShopCategoriesSettings);
    }

    [Fact]
    public async Task LoadTabActionResultIsInternalServerErrorWhenGetShopImportSettingsThrowsException()
    {
        var exception = new InvalidOperationException("test error");
        _settingsDataAdapterMock.Setup(s => s.GetShopImportSettings(It.IsAny<int>(), It.IsAny<ShopSettingType>())).Throws(exception);

        await SetShopsAsync();

        var homeController = CreateHomeController();

        var tabType = TabType.Shop;
        var tabView = TabHelper.TabViewNames.FirstOrDefault(d => d.Key == tabType).Value;
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var result = Assert.IsType<ObjectResult>(await homeController.LoadTab(shopGuid, (int)tabType, tabView));
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        Assert.Equal(exception, Assert.IsType<InvalidOperationException>(result.Value));
    }

    [Fact]
    public async Task LoadTabActionResultModelIsShopProductSettingsWhenShopGuidAndShopSettingsTabProductSelected()
    {        
        await SetShopsAsync();

        var shopImport = _importFacade.GetShops().Last();
        await SetShopSettingAsync(shopImport.ShopGuid, ShopSettingType.Product);
        
        var homeController = CreateHomeController();

        var tabType = TabType.Shop;
        var tabView = "ShopSettings.cshtml";
        var view = Assert.IsType<PartialViewResult>(await homeController.LoadTab(shopImport.ShopGuid, (int)tabType, tabView));
        Assert.Equal(view.ViewName, $"~/Views/Home/{tabView}.cshtml");
        var model = Assert.IsType<ShopSettingTabsModel>(view.Model);
        Assert.Equal(shopImport.ShopSettingTabs.Guid, model.Guid);
    }
}
