using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Controllers;
using Alchemist.Product.Import.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Alchemist.Product.Import.WebApp.Controller.Test;

public class HomeControllerTest:ControllerTest<HomeController>
{
    [Fact]
    public async Task IndexActionLoadShopsAsync()
    {
        var indexViewModel = await GetIndexActionViewModelAsync();
              
        Assert.Equal(_shops.Count, indexViewModel.Shops.Count);
    }

    [Fact]
    public async Task IndexActionDefaultTabIsShopSettingsAsync()
    {
        var indexViewModel = await GetIndexActionViewModelAsync();

        Assert.Equal(_shops.Count, indexViewModel.Shops.Count);
        Assert.Equal(TabType.Shop, indexViewModel.SelectedTab);
    }

    [Fact]
    public async Task IndexActionDefaultShopSettingsIsProductsAsync()
    {
        var indexViewModel = await GetIndexActionViewModelAsync(); 

        var shopGuid = indexViewModel.SelectedShopImport.ShopGuid;
        Assert.NotEqual(Guid.Empty, shopGuid);

        var shopSettings = indexViewModel.SelectedShopImport.GetSettings(indexViewModel.SelectedTab, true);
        Assert.NotNull(shopSettings);
        Assert.IsType<ProductShopSettingsModel>(shopSettings);
        Assert.Equal(shopGuid, shopSettings.ShopGuid);
    }

    [Fact]
    public async Task IndexActionWhenSelectedShopChangedAsync()
    {
        var homeController = CreateHomeController();

        var indexViewModel = await CommonActions.GetIndexActionViewModelAsync(homeController);

        var currentShopGuid = indexViewModel.SelectedTabModel.ShopGuid;
        var nextShopGuid = indexViewModel.Shops.FirstOrDefault(s => s.Guid != currentShopGuid)?.Guid;
        Assert.NotNull(nextShopGuid);
        Assert.NotEqual(Guid.Empty,nextShopGuid);

        var nextIndexView = await homeController.Index((Guid)nextShopGuid, (int)indexViewModel.SelectedTab) as ViewResult;
        Assert.NotNull(nextIndexView);
        var nextIndexViewModel = nextIndexView.Model as IndexViewModel;
        Assert.NotNull(nextIndexViewModel);
        Assert.Equal(indexViewModel.SelectedTab, nextIndexViewModel.SelectedTab);
        Assert.NotEqual(indexViewModel.SelectedTabModel.ShopGuid, nextIndexViewModel.SelectedTabModel.ShopGuid);
        Assert.Equal(nextShopGuid, nextIndexViewModel.SelectedTabModel.ShopGuid);
    }

    [Fact]
    public async Task IndexActionWhenSelectedTabChangedAsync()
    {
        var homeController = CreateHomeController();

        var indexViewModel = await CommonActions.GetIndexActionViewModelAsync(homeController);

        var currentTab = indexViewModel.SelectedTab;
        var nextTab = ViewHelper.Tabs.FirstOrDefault(t=>t != currentTab);
        var shopGuid = indexViewModel.SelectedTabModel.ShopGuid;

        var nextIndexView = await homeController.Index(shopGuid, (int)nextTab) as ViewResult;

        Assert.NotNull(nextIndexView);
        var nextIndexViewModel = nextIndexView.Model as IndexViewModel;
        Assert.NotNull(nextIndexViewModel);
        Assert.NotEqual(indexViewModel.SelectedTab, nextIndexViewModel.SelectedTab);
        Assert.NotEqual(indexViewModel.SelectedTabModel.Tab, nextIndexViewModel.SelectedTabModel.Tab);
        Assert.Equal(shopGuid, nextIndexViewModel.SelectedTabModel.ShopGuid);
    }


    [Fact]
    public async Task IndexActionBadRequestWhenNonexistentShopSetAsync()
    {
        var homeController = CreateHomeController();

        var indexViewModel = await CommonActions.GetIndexActionViewModelAsync(homeController);

        var tab = indexViewModel.SelectedTab;        
        var nextShopGuid = Guid.NewGuid();
        Assert.IsType<BadRequestResult>(await homeController.Index(nextShopGuid, (int)tab));
    }

    [Fact]
    public async Task IndexActionBadRequestWhenNonexistentTabSetAsync()
    {
        var homeController = CreateHomeController();

        var indexViewModel = await CommonActions.GetIndexActionViewModelAsync(homeController);

        var shopGuid = indexViewModel.SelectedTabModel.ShopGuid;
        var nextTab = ViewHelper.Tabs.Max() + 1;
        Assert.IsType<BadRequestResult>(await homeController.Index(shopGuid, (int)nextTab));
    }

    [Fact]
    public async Task SavePreviousTabModelWhenIndexActionAsync()
    {
        var homeController = CreateHomeController();

        var indexViewModel = await CommonActions.GetIndexActionViewModelAsync(homeController);

        var shopGuid = indexViewModel.SelectedShopImport.ShopGuid;  
        var shopSettings = indexViewModel.SelectedShopImport.GetSettings(indexViewModel.SelectedTab, true) as ShopSettingsModel;
        Assert.NotNull(shopSettings);

        var oldSettings = shopSettings.GetCopy();

        var savingShopSettings = shopSettings.GetCopy();
        savingShopSettings.FillShopSettingsFields();

        Assert.IsType<OkResult>(homeController.SaveTabSettings(shopGuid, (int)savingShopSettings.Tab, JsonSerializer.Serialize(savingShopSettings)));
        Assert.True(_importFacade.TryGetShopSettings(shopGuid, shopSettings.ShopSettingType, out var savedShopSettings));
        Assert.Equal(shopSettings.Guid, savedShopSettings.Guid);

        ModelAssert.EqualFields(savingShopSettings, savedShopSettings);
        ModelAssert.NotEqualFields(oldSettings, savedShopSettings);
    }
}
