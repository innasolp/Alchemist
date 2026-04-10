using Alchemist.Import.Products.Interfaces;
using Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure;
using Alchemist.Test.ImportSettingsWebApp.Factory;
using Test.PostresqlTestContainer;
using Xunit.Abstractions;
using TestCommon = Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure.Common;

namespace Alchemist.Product.ImportSettingsWebApp.Test;

public class SaveProductShopSettingsTestImportSettingsWebAppFactory()
    : ImportSettingsShopClientConfigurationWebAppFactory<PostgresqlTestDbContainer, PostgresDbRespawner>(false,
        8430, 8431,
        TestCommon.SignalRTestServer,
        8082, 8083,
        "ConnectionStrings:DbContext2",
        Common.ConfigurationHelper.GetSectionValue("SaveProductSettingsTestDb"),
        "ConnectionStrings:DbContext2",
        Common.ConfigurationHelper.GetSectionValue("SaveProductSettingsTestDb"),
        8108, 8109,
        8330, 8331,
        fillSettingsTestData: (context) => TestCommon.FillTestData(context, [1, 2, 3, 4]))
{
}

public class SaveProductShopSettingsTest : SaveShopImportSettingsTest<SaveProductShopSettingsTestImportSettingsWebAppFactory, 
    (string, string, string, PathFormatType, PathFormatType)>
{
    private readonly SaveProductShopSettingsTestImportSettingsWebAppFactory _webAppFactory;

    public SaveProductShopSettingsTest(SaveProductShopSettingsTestImportSettingsWebAppFactory webAppFactory, ITestOutputHelper outputHelper)
        :base(outputHelper)
    {
        _webAppFactory = webAppFactory;

        _webAppFactory.CreateClient();
    }

    protected override async Task ExpectSettingsLoadedAsync()
    {
        await this.ExpectProductShopSettingsLoadedAsync();
    }

    protected override async Task ExpectInputFieldsAsync(
        (string,
        string,
        string,
        PathFormatType,
        PathFormatType) inputs)
    {
        await Expect(Page.Locator($"#ShopSettingsName")).ToHaveValueAsync(inputs.Item1);
        await Expect(Page.Locator($"#ProductUrlFormat")).ToHaveValueAsync(inputs.Item2);
        await Expect(Page.Locator($"#CategoryUrlFormat")).ToHaveValueAsync(inputs.Item3);
        await Expect(Page.Locator($"#ProductUrlFormatType")).ToHaveValueAsync(((int)inputs.Item4).ToString());
        await Expect(Page.Locator($"#CategoryUrlFormatType")).ToHaveValueAsync(((int)inputs.Item5).ToString());
    }

    protected override async Task ExpectPageLoadedAsync()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();
    }

    protected override async Task<(string, string, string, PathFormatType, PathFormatType)> FillInputFieldsAsync()
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
