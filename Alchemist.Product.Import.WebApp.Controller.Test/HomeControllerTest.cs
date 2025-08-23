using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Controllers;
using Alchemist.Product.Import.WebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Data;
using System.Text.Json;
using ShopSettingType = Alchemist.Import.Settings.Interfaces.ShopSettingType;

namespace Alchemist.Product.Import.WebApp.Controller.Test;

public class HomeControllerTest : ControllerTest<HomeController>
{
    private async Task<ViewResult> IndexActionIsTypeViewResultAsync()
    {
        var homeController = CreateHomeController();
        var actionResult = homeController.Index();
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
        var actionResult = Assert.IsType<OkObjectResult>(await homeController.UpdateShopsAsync());
        Assert.Equal(_shops.Count, Assert.IsAssignableFrom<List<IShopModel>>(actionResult.Value).Count);
    }

    [Fact]
    public async Task UpdateShopsActionResultIsOkShopModelList()
    {
        var homeController = CreateHomeController();

        var actionResult = Assert.IsType<OkObjectResult>(await homeController.UpdateShopsAsync());
        Assert.IsAssignableFrom<List<IShopModel>>(actionResult.Value);       
    }

    [Fact]
    public async Task UpdateShopsActionResultIsLoadedShopModels()
    {
        await LoadShopsAsync();

        var shops = _importFacade.GetShops().Select(s=>s.Shop).ToList();

        var homeController = CreateHomeController();
        
        var actionResult = Assert.IsType<OkObjectResult>(await homeController.UpdateShopsAsync());
        var model = Assert.IsAssignableFrom<List<IShopModel>>(actionResult.Value);
        Assert.True(model.All(s => shops.Any(s1 => s1.Guid == s.Guid)));
    }

    [Fact]
    public async Task UpdateShopsActionResultIsInternalServerErrorWhenLoadShopsThrowExceptionAsync()
    {
        var exception = new InvalidOperationException("test error");
        var homeController = CreateHomeController();
        _shopDataServiceMock.Setup(s => s.GetShops()).Throws(exception);
        
        var actionResult = Assert.IsType<ObjectResult>(await homeController.UpdateShopsAsync());
        Assert.Equal(exception.Message, Assert.IsType<InvalidOperationException>(actionResult.Value).Message);        
    }    

    [Fact]
    public async Task IndexActionWhenSelectedShopChangedAsync()
    {
        var homeController = CreateHomeController();

        await LoadShopsAsync();

        var nextShopGuid = _importFacade.GetShops().Last().ShopGuid;
        var selectedTab = TabType.Shop;

        var nextIndexView = Assert.IsType<ViewResult>(homeController.IndexFromQuery(nextShopGuid, (int)selectedTab));
        var nextIndexViewModel = Assert.IsType<IndexViewModel>(nextIndexView.Model);
        Assert.Equal(selectedTab, nextIndexViewModel.SelectedTab);
        Assert.Equal(nextShopGuid, nextIndexViewModel.SelectedShopImport.ShopGuid);
    }

    [Fact]
    public async Task IndexActionWhenSelectedTabChangedAsync()
    {
        var homeController = CreateHomeController(); 

        await LoadShopsAsync();

        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var nextTab = TabType.Products;

        var nextIndexView = Assert.IsType<ViewResult>(homeController.IndexFromQuery(shopGuid, (int)nextTab));
        var nextIndexViewModel = Assert.IsType<IndexViewModel>(nextIndexView.Model);        
        Assert.Equal(nextTab, nextIndexViewModel.SelectedTab);
        Assert.Equal(shopGuid, nextIndexViewModel.SelectedShopImport.ShopGuid);
    }


    [Fact]
    public async Task IndexActionNotFoundWhenNonexistingShopSetAsync()
    {
        await LoadShopsAsync();

        var homeController = CreateHomeController();        

        var tab = TabType.Shop;
        var nextShopGuid = Guid.NewGuid();

        Assert.Equal(nextShopGuid,
            Assert.IsType<Guid>(Assert.IsType<NotFoundObjectResult>(homeController.IndexFromQuery(nextShopGuid, (int)tab)).Value));
    }

    [Fact]
    public async Task IndexActionBadRequestWhenNonexistentTabSetAsync()
    {
        var homeController = CreateHomeController();
        await LoadShopsAsync();

        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var nextTab = (int)Enum.GetValues<TabType>().Max() + 1;
        Assert.IsType<BadRequestResult>(homeController.IndexFromQuery(shopGuid, nextTab));
    }

    [Fact]
    public async Task IndexActionBadRequestWhenEmptyShopGuidSetAsync()
    {
        var homeController = CreateHomeController();
        await LoadShopsAsync();
        Assert.IsType<BadRequestResult>(homeController.IndexFromQuery(Guid.Empty, (int)TabType.Shop));
    }

    [Fact]
    public async Task IsTabChangedBadRequestWhenJsonIsEmpty()
    {
        var homeController = CreateHomeController();
        var result = Assert.IsType<BadRequestObjectResult>(await homeController.IsTabChangedAsync(Guid.NewGuid(), 1, string.Empty));
        Assert.Equal(string.Empty, Assert.IsType<string>(result.Value));
    }

    [Fact]
    public async Task IsTabChangedNotFoundWhenNonExistingShopGuid()
    {
        await LoadShopsAsync();
        var homeController = CreateHomeController();
        var guid = Guid.NewGuid();
        string json = Guid.NewGuid().ToString();
        var result = Assert.IsType<NotFoundObjectResult>(await homeController.IsTabChangedAsync(guid, 1, json));
        Assert.Equal(guid, Assert.IsType<Guid>(result.Value));
    }

    [Fact]
    public async Task IsTabChangedBadRequestWhenNonExistingTabSetting()
    {
        await LoadShopsAsync();
        var homeController = CreateHomeController();
        var guid = _importFacade.GetShops().First().ShopGuid;
        string json = Guid.NewGuid().ToString();

        var tab = Enum.GetValues(typeof(TabType)).Length + 1;
        var result = Assert.IsType<BadRequestObjectResult>(await homeController.IsTabChangedAsync(guid, tab, json));
        Assert.Equal(tab, Assert.IsType<int>(result.Value));
    }

    [Fact]
    public async Task IsTabChangedBadRequestWhenInvalidJson()
    {
        await LoadShopsAsync();
        var homeController = CreateHomeController();
        var guid = _importFacade.GetShops().First().ShopGuid;
        //var shopSettingsType = ShopSettingType.Product;
        string json = Guid.NewGuid().ToString();

        //await SetShopSettingAsync(guid, shopSettingsType);

        var result = Assert.IsType<BadRequestObjectResult>(await homeController.IsTabChangedAsync(guid, (int)TabType.Shop, json));
        Assert.Equal(json, Assert.IsType<string>(result.Value));
    }

    [Fact]
    public async Task IsTabChangedOkFalseWhenJsonEqualShopSettings()
    {
        await LoadShopsAsync();
        var homeController = CreateHomeController();

        var guid = _importFacade.GetShops().First().ShopGuid;
        var shopSettingsType = ShopSettingType.Product;      
        

        Assert.True(_importFacade.TryGetShopSettings(guid, shopSettingsType, out var shopSettings));
        FillShopSettingsFields(shopSettings);        

        var json = JsonSerializer.Serialize(shopSettings, typeof(ProductShopSettingsModel),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true, 
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString });

        var result = Assert.IsType<OkObjectResult>(await homeController.IsTabChangedAsync(guid, (int)TabType.Shop, json));
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
        var view = Assert.IsType<PartialViewResult>(homeController.ShopList(_shops.Select(s=>new ShopModel(s.Id) { Name = s.Name})));
        var shopModels = Assert.IsAssignableFrom<IEnumerable<ShopModel>>(view.Model);
        Assert.Equal(_shops.Count, shopModels.Count());
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
        Assert.True(model.ShopProductsSettings.IsEmpty());
        Assert.True(model.ShopCategoriesSettings.IsEmpty());
    }

    [Fact]
    public async Task TabsMenuActionResultModelIsFirstShopSettingsWhenEmptyShopGuid()
    {
        await LoadShopsAsync();

        var shopImport = _importFacade.GetShops().First();
        
        Assert.True(_importFacade.TryGetShopSettings(shopImport.ShopGuid, ShopSettingType.Product, out var shopSettings));
        FillShopSettingsFields(shopSettings);

        var homeController = CreateHomeController();
        var view = Assert.IsType<PartialViewResult>(homeController.TabsMenu(Guid.Empty, (int)TabType.Shop));
        var model = Assert.IsType<ShopSettingTabsModel>(view.Model);

        Assert.False(model.ShopProductsSettings.IsEmpty());
        Assert.Equal(shopImport.ShopSettingTabs.Guid, model.Guid);
    }

    [Fact]
    public async Task TabsMenuActionResultModelIsDefaultTabModelWhenNotExistingShopGuid()
    {
        await LoadShopsAsync();
        var homeController = CreateHomeController();
        var view = Assert.IsType<PartialViewResult>(homeController.TabsMenu(Guid.NewGuid(), (int)TabType.Shop));
        var model = Assert.IsType<ShopSettingTabsModel>(view.Model);
        Assert.True(model.ShopProductsSettings.IsEmpty());
        Assert.True(model.ShopCategoriesSettings.IsEmpty());
    }

    [Fact]
    public async Task TabsMenuActionResultModelIsTabModelShopGuid()
    {
        await LoadShopsAsync();

        var shopGuid = _importFacade.GetShops().Last().ShopGuid;

        Assert.True(_importFacade.TryGetShopSettings(shopGuid, ShopSettingType.Product, out var productShopSettings));
        Assert.True(_importFacade.TryGetShopSettings(shopGuid, ShopSettingType.Category, out var categoryShopSettings));

        FillShopSettingsFields(productShopSettings);
        FillShopSettingsFields(categoryShopSettings);
        
        var homeController = CreateHomeController();
        var view = Assert.IsType<PartialViewResult>(homeController.TabsMenu(shopGuid, (int)TabType.Shop));
        var model = Assert.IsType<ShopSettingTabsModel>(view.Model);
        Assert.True(model.ShopProductsSettings?.AllEquals(productShopSettings));
        Assert.True(model.ShopCategoriesSettings?.AllEquals(categoryShopSettings));
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
        var view = Assert.IsType<PartialViewResult>(await homeController.LoadTabAsync(Guid.NewGuid(), (int)tabType, viewName));
        Assert.Equal(view.ViewName, $"~/Views/Home/{viewName}.cshtml");
    }

    [Fact]
    public async Task LoadTabActionResultModelIsDefaultTabModelWhenNotExistingShopGuid()
    {
        await LoadShopsAsync();
        var homeController = CreateHomeController();
        var tabType = TabType.Shop;
        var tabView = TabHelper.TabViewNames.FirstOrDefault(d => d.Key == tabType).Value;
        var view = Assert.IsType<PartialViewResult>(await homeController.LoadTabAsync(Guid.NewGuid(), (int)tabType, tabView));
        var model = Assert.IsType<ShopSettingTabsModel>(view.Model);
        Assert.True(model.ShopProductsSettings.IsEmpty());
        Assert.True(model.ShopCategoriesSettings.IsEmpty());
    }

    [Fact]
    public async Task LoadTabActionResultIsInternalServerErrorWhenGetShopImportSettingsThrowsException()
    {
        var exception = new InvalidOperationException("test error");
        _productSettingsDataAdapterMock.Setup(s => s.GetShopImportSettings(It.IsAny<int>()))
            .Throws(exception);
        _categorySettingsDataAdapterMock.Setup(s => s.GetShopImportSettings(It.IsAny<int>()))
            .Throws(exception);

        await LoadShopsAsync();

        var homeController = CreateHomeController();

        var tabType = TabType.Shop;
        var tabView = TabHelper.TabViewNames.FirstOrDefault(d => d.Key == tabType).Value;
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var result = Assert.IsType<ObjectResult>(await homeController.LoadTabAsync(shopGuid, (int)tabType, tabView));
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        Assert.Equal(exception, Assert.IsType<InvalidOperationException>(result.Value));
    }

    [Fact]
    public async Task LoadTabActionResultModelIsShopProductSettingsWhenShopGuidAndShopSettingsTabProductSelected()
    {        
        await LoadShopsAsync();

        var shopImport = _importFacade.GetShops().Last();
        Assert.True(_importFacade.TryGetShopSettings(shopImport.ShopGuid, ShopSettingType.Product, out var shopSettings));
        FillShopSettingsFields(shopSettings);


        var homeController = CreateHomeController();

        var tabType = TabType.Shop;
        var tabView = "ShopSettings.cshtml";
        var view = Assert.IsType<PartialViewResult>(await homeController.LoadTabAsync(shopImport.ShopGuid, (int)tabType, tabView));
        Assert.Equal(view.ViewName, $"~/Views/Home/{tabView}.cshtml");
        var model = Assert.IsType<ShopSettingTabsModel>(view.Model);
        Assert.Equal(shopImport.ShopSettingTabs.Guid, model.Guid);
    }
}
