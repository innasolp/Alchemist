using Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure;
using Alchemist.Test.ImportSettingsWebApp.Factory;
using Xunit.Abstractions;
using TestCommon = Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure.Common;

namespace Alchemist.Product.ImportSettingsWebApp.Test;

public class SaveProductShopSettingsTestImportSettingsWebAppFactory()
    : ImportSettingsWebAppFactory(false,
        TestCommon.CreateShopWebAppApiFactory("SaveProductSettingsTestDb", 8430, 8431, 8082, 8083).ServerAddress,
        8108, 8109,
        TestCommon.CreateSettingsApiHttpClient("SaveProductSettingsTestDb", 8330, 8331))
{
}

public class SaveProductShopSettingsTest : SaveShopImportSettingsTest<SaveProductShopSettingsTestImportSettingsWebAppFactory, (string, string, string)>
{
    private readonly SaveProductShopSettingsTestImportSettingsWebAppFactory _webAppFactory;
    private readonly ITestOutputHelper _outputHelper;

    public SaveProductShopSettingsTest(SaveProductShopSettingsTestImportSettingsWebAppFactory webAppFactory, ITestOutputHelper outputHelper)
    {
        _webAppFactory = webAppFactory;
        _outputHelper = outputHelper;

        _webAppFactory.CreateClient();
    }

    protected override async Task ExpectSettingsLoadedAsync()
    {
        await this.ExpectProductShopSettingsLoadedAsync();
    }

    protected override async Task ExpectInputFieldsAsync((string, string, string) inputs)
    {
        await Expect(Page.Locator($"#ShopSettingsName")).ToHaveValueAsync(inputs.Item1);
        await Expect(Page.Locator($"#ProductUrlFormat")).ToHaveValueAsync(inputs.Item2);
        await Expect(Page.Locator($"#CategoryUrlFormat")).ToHaveValueAsync(inputs.Item3);
    }

    protected override async Task ExpectPageLoadedAsync()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();
    }

    protected override async Task<(string, string, string)> FillInputFieldsAsync()
    {
        return await this.FillProductInputFieldsAsync();
    }

    protected override Task SelectOtherTabAsync()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task FieldsEqualsInputValuesWhenSettingsSaved()
    {
        await FieldsEqualsInputValuesWhenSettingsSavedAsync();
    }
}
