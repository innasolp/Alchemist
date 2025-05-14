using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.WebApp.Controllers;
using Alchemist.Product.Import.WebApp.Models;
using Alchemist.Import.Settings.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Alchemist.Product.Import.Model.Infrastructure;

namespace Alchemist.Product.Import.WebApp.Controller.Test;

public static class CommonActions
{
    public static async Task<IndexViewModel> GetIndexActionViewModelAsync(HomeController homeController)
    {
        var view = await GetIndexActionViewAsync(homeController);

        Assert.NotNull(view.Model);
        return Assert.IsType<IndexViewModel>(view.Model);
    }

    public static async Task<ViewResult> GetIndexActionViewAsync(HomeController homeController)
    {
        var actionResult = await homeController.Index();
        Assert.NotNull(actionResult);
        return Assert.IsType<ViewResult>(actionResult);
    }

    public static async Task<IndexViewModel> GetIndexActionViewModelAfterUpdateShopsAsync(HomeController homeController)
    {
        var actionResult = Assert.IsType<OkObjectResult>(await homeController.UpdateShops());
        Assert.True(Assert.IsType<bool>(actionResult.Value));

        return await GetIndexActionViewModelAsync(homeController);
    }

    public static async Task<TShopSettings> ChangeShopSettingsAsync<TShopSettings>(HomeController homeController,
        ShopSettingsController shopSettingController, 
        ShopSettingType shopSettingType)
        where TShopSettings : ShopSettingsModel, new()
    {
        var indexViewModel = await GetIndexActionViewModelAfterUpdateShopsAsync(homeController);

        var productSettings = Assert.IsType<ProductShopSettingsModel>(indexViewModel.SelectedShopImport.GetSettings(indexViewModel.SelectedTab, true));

        var actionResult = Assert.IsType<OkObjectResult>(shopSettingController.SetShopSettings(productSettings.ShopGuid, (int)shopSettingType));
        Assert.True(Assert.IsType<bool>(actionResult.Value));

        Assert.Equal(ShopSettingType.Category, indexViewModel.SelectedShopImport.ShopSettingTabs.SelectedSettingsTab);

        await homeController.IndexFromQueryAsync(productSettings.ShopGuid, (int)indexViewModel.SelectedTab);
        return Assert.IsType<TShopSettings>(indexViewModel.SelectedShopImport.ShopSettingTabs.GetShopSettingsByType(shopSettingType));
    }
}
