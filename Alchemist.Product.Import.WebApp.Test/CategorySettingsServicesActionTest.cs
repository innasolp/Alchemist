using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Model.Infrastructure;
using Microsoft.Playwright;
using System.Text.Json;
using Xunit.Abstractions;
using Alchemist.Product.Import.WebApp.Test.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;

namespace Alchemist.Product.Import.WebApp.Test;

public class CategorySettingsServicesActionTest(TestImportWebAppFactory webAppFactory, ITestOutputHelper testOutputHelper) 
    : ImportWebAppTest(webAppFactory, testOutputHelper, 8118, 8119)
{
    private async Task ExpectSopSettingsCategoryTabAsync(IPage page)
    {
        var tabsMenuDiv = await this.ExpectSingleElementAsync(page, ".tabsMenuDiv");

        await Expect(tabsMenuDiv.Locator(".menu_header")).ToHaveCountAsync(2);

        var categoryTabItemDiv = tabsMenuDiv.Locator(".menu_header").GetByText(TabHelper.ShopSettingTypeNames[ShopSettingType.Category]);
        await Expect(categoryTabItemDiv).ToHaveCountAsync(1);
        await categoryTabItemDiv.ClickAsync();

        await Task.Delay(500);

        var settingsForm = page.Locator("#settingsForm");

        await this.ExpectSingleElementAsync(settingsForm, "#CategorySourceUrl");
    }

    private async Task<ILocator> ExpectServiceTableAsync(IPage page)
    {
        var settingsForm = page.Locator("#settingsForm");

        var serviceSettingsTable = await this.ExpectSingleElementAsync(settingsForm, ".services");        

        return serviceSettingsTable;
    }

    private async Task<ILocator> ExpectShowServiceFormWhenAddServiceButtonClickAsync(IPage page, ILocator serviceTable)
    {
        var serviceSettingsForm = page.Locator("#serviceSettingsForm");
        await Expect(serviceSettingsForm).Not.ToBeVisibleAsync();

        var addServiceButton = await this.ExpectSingleElementAsync(serviceTable, ".addService");        

        await addServiceButton.ClickAsync();

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

    private void SetSecondaryServices(Interfaces.IShopSettings shopSetting)
    {
        var categoryShopImportSettings = new CategoryShopSettingsModel(shopSetting.ShopId, shopSetting.Id, Guid.NewGuid())
        {
            Name = shopSetting.Name,
            CategorySourceUrl = $"https://url{shopSetting.Id}_category"
        };
        shopSetting.JsonValue = JsonSerializer.Serialize(categoryShopImportSettings);

        _shopSettings.Add(CreateServiceSettings(shopSetting.ShopId, shopSetting.Id, _shopSettings.Count));
        _shopSettings.Add(CreateServiceSettings(shopSetting.ShopId, shopSetting.Id, _shopSettings.Count));
    }    


    [Fact]
    public async Task CategoryShopSettingsHasServiceTable()
    {
        var newPage = await Context.NewPageAsync();       

        await ExpectLoadIndexPageAsync(newPage);

        await ExpectSopSettingsCategoryTabAsync(newPage);

        await ExpectServiceTableAsync(newPage);
    }

    [Fact]
    public async Task ServiceModalWhenAddServiceButtonClick()
    {
        var newPage = await Context.NewPageAsync();

        await ExpectLoadIndexPageAsync(newPage);

        await ExpectSopSettingsCategoryTabAsync(newPage);

        var serviceTable = await ExpectServiceTableAsync(newPage);

        var serviceSettingsForm = await ExpectShowServiceFormWhenAddServiceButtonClickAsync(newPage, serviceTable);

        var serviceName = serviceSettingsForm.Locator("#ServiceName");
        await Expect(serviceName).ToBeVisibleAsync();
        await Expect(serviceName).ToContainClassAsync("form-control");
        await Expect(serviceName).ToContainClassAsync(" form-input");
    }

    [Fact]
    public async Task ServiceTableContainsRowsWhenSecondaryServicesExists()
    {
        var shopSetting = _shopSettings[2];

        SetSecondaryServices(shopSetting);

        var newPage = await Context.NewPageAsync();

        var shopSettings = await ExpectLoadIndexPageAsync(newPage);        

        await ExpectSopSettingsCategoryTabAsync(newPage);

        var serviceTable = await ExpectServiceTableAsync(newPage);

        await Task.Delay(500);

        var services = _shopSettings.Where(s => s.ParentSettingsId == shopSetting.Id && !s.Name.IsPrimaryServiceName())
                .Select(s => s.ToImportServiceSettings<ServiceSettingsModel>()).ToArray();
        await ExpectRowsInServiceTableAsync(serviceTable, services);
    }

    [Fact]
    public async Task RowAddToServiceTableWhenNewServiceAdded()
    {
        await Context.ClearCookiesAsync();

        var newPage = await Context.NewPageAsync();

        await ExpectLoadIndexPageAsync(newPage);

        await ExpectSopSettingsCategoryTabAsync(newPage);

        var serviceTable = await ExpectServiceTableAsync(newPage);

        var shopImportSettings = await GetShopImportSettingsAsync(_shops.First().Id, Interfaces.ShopSettingType.Category);
        var categoryShopSettings = Assert.IsType<CategoryShopSettingsModel>(shopImportSettings);

        var serviceSettingsForm = await ExpectShowServiceFormWhenAddServiceButtonClickAsync(newPage, serviceTable);

        var serviceSettings = _shopSettings.FirstOrDefault(s => s.ParentSettingsId == shopImportSettings.Id && s.Name.IsPrimaryServiceName())?
            .ToImportServiceSettings<ServiceSettingsModel>()
           ?? new ServiceSettingsModel(shopImportSettings.ShopId, 0, shopImportSettings.Id, shopImportSettings.Guid, shopImportSettings.ShopGuid)
           {
               Name = Guid.NewGuid().ToString()
           };
        serviceSettings = await serviceSettingsForm.FillServiceSettingsInputsAsync(serviceSettings, "");

        var saveButton = serviceSettingsForm.Locator("button[class='btn btn-primary']");
        await saveButton.ClickAsync();

        await Expect(serviceSettingsForm).Not.ToBeVisibleAsync();

        await ExpectRowsInServiceTableAsync(serviceTable, [serviceSettings]);
    }

    [Fact]
    public async Task ServiceRowChangedWhenServiceWasEdited()
    {
        var shopSetting = _shopSettings[2];

        SetSecondaryServices(shopSetting);

        var newPage = await Context.NewPageAsync();

        var shopSettings = await ExpectLoadIndexPageAsync(newPage);

        await ExpectSopSettingsCategoryTabAsync(newPage);

        var serviceTable = await ExpectServiceTableAsync(newPage);

        var rows = await serviceTable.Locator(".serviceRow").AllAsync();

        var services = _shopSettings.Where(s => s.ParentSettingsId == shopSetting.Id && !s.Name.IsPrimaryServiceName()).ToList();
        var service = services.Last().ToImportServiceSettings<ServiceSettingsModel>();
        
        ILocator? serviceRow = null;
        foreach (var row in rows)
        {
            if (await row.Locator(".name").TextContentAsync() == service.Name
                && await row.Locator(".serviceTypeName").TextContentAsync() == service.ServiceTypeName)
            {
                serviceRow = row;
                break;
            }
        }

        Assert.NotNull(serviceRow);

        await Expect(serviceRow).ToHaveCountAsync(1);

        var buttonEdit = await this.ExpectSingleElementAsync(serviceRow, ".editService");

        var serviceForm = await this.ExpectShowServiceModalFormAsync(newPage, (page) => Task.FromResult(buttonEdit));

        service = await serviceForm.FillServiceSettingsInputsAsync(service, service.Name);

        var saveButton = await this.ExpectSingleElementAsync(serviceForm, "button[class='btn btn-primary']");        

        await saveButton.ClickAsync();

        await Expect(serviceForm).Not.ToBeVisibleAsync();
        
        await Expect(serviceRow.Locator(".serviceTypeName")).ToContainTextAsync(service.ServiceTypeName);
    }
}
