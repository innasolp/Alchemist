using Microsoft.Playwright;
using Xunit.Abstractions;

namespace Alchemist.Product.Import.WebApp.Test;

public class ProductShopSettingsActionTest(TestImportWebAppFactory webAppFactory, ITestOutputHelper testOutputHelper)
    : ImportWebAppTest(webAppFactory, testOutputHelper, 8122, 8123)
{
    [Fact]
    public async Task ValidationFailedWhenRequiredFieldsNotFilled()
    {
        var newPage = await Context.NewPageAsync();

        await ExpectLoadIndexPageAsync(newPage);

        var settingsForm = newPage.Locator("#settingsForm");

        await Expect(settingsForm.Locator("#ShopSettingsName-error")).Not.ToBeVisibleAsync();
        await Expect(settingsForm.Locator("#ProductUrlFormat-error")).Not.ToBeVisibleAsync();
        await Expect(settingsForm.Locator("#CategoryUrlFormat-error")).Not.ToBeVisibleAsync();

        await settingsForm.Locator("#ShopSettingsName").FillAsync("");
        await settingsForm.Locator("#ProductUrlFormat").FillAsync("");
        await settingsForm.Locator("#CategoryUrlFormat").FillAsync("");

        var saveButton = settingsForm.GetByRole(AriaRole.Button).GetByText("Save");
        await saveButton.ClickAsync();

        await Expect(settingsForm.Locator("#ShopSettingsName-error")).ToBeVisibleAsync();
        await Expect(settingsForm.Locator("#ProductUrlFormat-error")).ToBeVisibleAsync();
        await Expect(settingsForm.Locator("#CategoryUrlFormat-error")).ToBeVisibleAsync();
    }
}
