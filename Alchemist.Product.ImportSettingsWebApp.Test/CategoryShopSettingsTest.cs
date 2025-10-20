using Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure;
using Alchemist.Test.ImportSettingsWebApp.Factory;
using Xunit.Abstractions;
using TestCommon = Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure.Common;

namespace Alchemist.Product.ImportSettingsWebApp.Test;

public class CategoryShopSettingsTestImportSettingsWebAppFactory()
    : ImportSettingsWebAppFactory(false,
        TestCommon.CreateShopWebAppApiFactory("CategorySettingsTestDb", 8424, 8425, 8076, 8077).ServerAddress,
        8094, 8095,
        TestCommon.CreateSettingsApiHttpClient("CategorySettingsTestDb", 8224, 8225))
{
}

public class CategoryShopSettingsTest : ShopImportSettingsTest<CategoryShopSettingsTestImportSettingsWebAppFactory>
{
    private readonly CategoryShopSettingsTestImportSettingsWebAppFactory _webAppFactory;
    private readonly ITestOutputHelper _outputHelper;

    public CategoryShopSettingsTest(CategoryShopSettingsTestImportSettingsWebAppFactory webAppFactory, ITestOutputHelper outputHelper)
    {
        _webAppFactory = webAppFactory;
        _outputHelper = outputHelper;

        _webAppFactory.CreateClient();
    }

    protected override async Task ExpectSettingsLoadedAsync()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await this.SettingsTabClickAsync("Category");

        await this.ExpectCategoryShopSettingsLoadedAsync();
    }

    protected override async Task FillInputFieldsAsync()
    {
        await Page.Locator($"#CategorySourceUrl").FillAsync(Guid.NewGuid().ToString());
    }

    protected override async Task SelectOtherTabAsync()
    {
        await this.SettingsTabClickAsync("Product");
    }

    [Fact]
    public async Task PopupConfirmationWhenSelectOtherShopAfterMakingChanges()
    {
        await PopupConfirmationWhenSelectOtherShopAfterMakingChangesAsync();
    }

    [Fact]
    public async Task PopupConfirmationWhenSelectOtherTabAfterMakingChanges()
    {
        await PopupConfirmationWhenSelectOtherTabAfterMakingChangesAsync();
    }

    [Fact]
    public async Task ValidationErrorWhenShopSettingsNameIsEmpty()
    {
        await ValidationErrorWhenShopSettingsNameIsEmptyAsync();
    }

    [Fact]
    public async Task ValidationErrorWhenCategorySourceUrlIsEmpty()
    {
        await ValidationErrorWhenRequiredFieldIsEmptyAsync("CategorySourceUrl", "CategorySourceUrl");
    }
    

    [Fact]
    public async Task ValidationErrorWhenImportServiceNotSet()
    {
        await ValidationErrorWhenImportServiceNotSetAsync();
    }

    [Fact]
    public async Task ValidationErrorWhenWebLoaderNotSet()
    {
        await ValidationErrorWhenWebLoaderNotSetAsync();
    }
}