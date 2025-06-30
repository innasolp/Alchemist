using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Model.Infrastructure;
using Microsoft.Playwright;
using System.Text.Json;
using Xunit.Abstractions;
using Alchemist.Product.Import.WebApp.Test.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;
using ModelHelper = Alchemist.Product.Import.WebApp.Models.ModelHelper;

namespace Alchemist.Product.Import.WebApp.Test;

public class CategorySettingsServicesActionTest(TestImportWebAppFactory webAppFactory, ITestOutputHelper testOutputHelper) 
    : ImportWebAppTest(webAppFactory, testOutputHelper, 8118, 8119)
{
    private async Task<ILocator> ExpectServiceTableOnShopCategoryTabAsync(IPage page)
    {
        var tabsMenuDiv = page.Locator("#tabsMenuDiv");

        await Task.Delay(500);

        var categoryTabItemDiv = tabsMenuDiv.Locator("div").GetByText(TabHelper.ShopSettingTypeNames[ShopSettingType.Category]);
        await Expect(categoryTabItemDiv).ToHaveCountAsync(1);
        await categoryTabItemDiv.ClickAsync();

        await Task.Delay(500);

        var settingsForm = page.Locator("#settingsForm");

        await Expect(settingsForm.Locator("#CategorySourceUrl")).ToHaveCountAsync(1);

        var serviceSettingsTable = settingsForm.Locator("#servicesUl");
        await Expect(serviceSettingsTable).ToHaveCountAsync(1);

        return serviceSettingsTable;
    }

    private async Task<ILocator> ExpectShowServiceFormWhenAddServiceButtonClickAsync(IPage page, ILocator serviceTable)
    {
        var serviceSettingsForm = page.Locator("#serviceSettingsForm");
        await Expect(serviceSettingsForm).Not.ToBeVisibleAsync();

        var addService = serviceTable.Locator("#addService");
        await Expect(addService).ToHaveCountAsync(1);

        await addService.ClickAsync();

        await Expect(serviceSettingsForm).ToBeVisibleAsync();

        return serviceSettingsForm;
    }

    private async Task ExpectRowsInServiceTableAsync(ILocator serviceTable, ServiceSettingsModel[] services)
    {
        var servicesRows = serviceTable.Locator(".service_li");
        await Expect(servicesRows).ToHaveCountAsync(services.Length);

        var rows = (await servicesRows.AllAsync()).ToArray();
        for (var i = 0; i < rows.Length; i++)
        {
            var item = rows[i].Locator(".name");
            await Expect(item).ToHaveTextAsync(services[i].Name ?? "");

            var url = rows[i].Locator(".serviceTypeName");
            await Expect(url).ToHaveTextAsync(services[i].ServiceTypeName ?? "");
        }
    }

    [Fact]
    public async Task CategoryShopSettingsHasServiceTable()
    {
        var newPage = await Context.NewPageAsync();

        await ExpectLoadIndexPageAsync(newPage);

        await ExpectServiceTableOnShopCategoryTabAsync(newPage);
    }

    [Fact]
    public async Task ServiceModalWhenAddServiceButtonClick()
    {
        var newPage = await Context.NewPageAsync();

        await ExpectLoadIndexPageAsync(newPage);

        var serviceTable = await ExpectServiceTableOnShopCategoryTabAsync(newPage);

        var serviceSettingsForm = await ExpectShowServiceFormWhenAddServiceButtonClickAsync(newPage, serviceTable);

        var serviceName = serviceSettingsForm.Locator("#ServiceName");
        await Expect(serviceName).ToBeVisibleAsync();
        await Expect(serviceName).ToContainClassAsync("form-control");
        await Expect(serviceName).ToContainClassAsync(" form-input");
    }

    [Fact]
    public async Task ServiceTableContainsRowsWhenRootCategoriesExists()
    {
        var shopSetting = _shopSettings[2];
        var categoryShopImportSettings = new CategoryShopSettingsModel()
        {
            Name = shopSetting.Name,
            CategorySourceUrl = $"https://url{shopSetting.Id}_category"            
        };
        shopSetting.JsonValue = JsonSerializer.Serialize(categoryShopImportSettings);

        _shopSettings.Add(CreateServiceSettings(shopSetting, _shopSettings.Count));
        _shopSettings.Add(CreateServiceSettings(shopSetting, _shopSettings.Count));        

        var newPage = await Context.NewPageAsync();

        await ExpectLoadIndexPageAsync(newPage);        

        var serviceTable = await ExpectServiceTableOnShopCategoryTabAsync(newPage);

        await Task.Delay(500);

        var services = _shopSettings.Where(s => s.ParentSettingsId == shopSetting.Id && !ModelHelper.IsServiceSettingsPrimary(s.Name))
                .Select(s => s.ToImportServiceSettings<ServiceSettingsModel>()).ToArray();
        await ExpectRowsInServiceTableAsync(serviceTable, services);
    }

    [Fact]
    public async Task RowAddToServiceTableWhenNewServiceAdded()
    {
        await Context.ClearCookiesAsync();

        var newPage = await Context.NewPageAsync();

        await ExpectLoadIndexPageAsync(newPage);

        var serviceTable = await ExpectServiceTableOnShopCategoryTabAsync(newPage);

        var shopImportSettings = await GetShopImportSettingsAsync(_shops.First().Id, Interfaces.ShopSettingType.Category);
        var categoryShopSettings = Assert.IsType<CategoryShopSettingsModel>(shopImportSettings);

        var serviceSettingsForm = await ExpectShowServiceFormWhenAddServiceButtonClickAsync(newPage, serviceTable);        

        var serviceSettings = await serviceSettingsForm.FillServiceSettingsInputsAsync(_shopSettings, "", categoryShopSettings);

        var saveButton = serviceSettingsForm.Locator("button[class='btn btn-primary']");
        await saveButton.ClickAsync();

        await Expect(serviceSettingsForm).Not.ToBeVisibleAsync();

        await ExpectRowsInServiceTableAsync(serviceTable, [serviceSettings]);
    }
}
