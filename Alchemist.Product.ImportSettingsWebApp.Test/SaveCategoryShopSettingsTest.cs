using Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure;
using Xunit.Abstractions;

namespace Alchemist.Product.ImportSettingsWebApp.Test;

public class SaveCategoryShopSettingsTestImportSettingsWebAppFactory()
    : ImportSettingsWebAppTestContainerLifetimeFactory(false,
        8098, 8099,
        8228, 8229,
        Common.ConfigurationHelper.GetSectionValue("SaveCategorySettingsTestDb"),
        8428, 8429, 8080, 8081)
{
}

public class SaveCategoryShopSettingsTest : 
    SaveShopImportSettingsTest<SaveCategoryShopSettingsTestImportSettingsWebAppFactory, (string, string)>
{
    private readonly SaveCategoryShopSettingsTestImportSettingsWebAppFactory _webAppFactory;    

    public SaveCategoryShopSettingsTest(SaveCategoryShopSettingsTestImportSettingsWebAppFactory webAppFactory, ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _webAppFactory = webAppFactory;        

        _webAppFactory.CreateClient();
    }

    protected override async Task ExpectSettingsLoadedAsync()
    {
        await this.ExpectCategoryShopSettingsLoadedAsync();
    }

    protected override async Task ExpectInputFieldsAsync((string, string) inputs)
    {
        await Expect(Page.Locator($"#ShopSettingsName")).ToHaveValueAsync(inputs.Item1);
        await Expect(Page.Locator($"#CategorySourceUrl")).ToHaveValueAsync(inputs.Item2);
    }

    protected override async Task SelectOtherTabAsync()
    {
        await this.SettingsTabClickAsync("Product");
    }

    protected override async Task ExpectPageLoadedAsync()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await this.SettingsTabClickAsync("Category");

        await this.ExpectCategoryShopSettingsLoadedAsync();
    }

    protected override async Task<(string, string)> FillInputFieldsAsync()
    {
        return await this.FillCategoryInputFieldsAsync();
    }

    [Fact]
    public async Task FieldsEqualsInputValuesWhenSettingsSaved()
    {
        await FieldsEqualsInputValuesWhenSettingsSavedAsync();
    }
}
