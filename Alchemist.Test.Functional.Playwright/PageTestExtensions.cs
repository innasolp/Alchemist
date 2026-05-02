using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace Alchemist.Test.Functional.Playwright;

public static class PageTestExtensions
{
    public static async Task ExpectWithNullValueAsync(this PageTest pageTest, ILocator locator, string? value)
    {
        await pageTest.Expect(locator).ToBeVisibleAsync();

        if (!string.IsNullOrEmpty(value))
            await pageTest.Expect(locator).ToHaveValueAsync(value);
        else
            await pageTest.Expect(locator).ToHaveValueAsync("");
    }

    public static async Task<ILocator> ExpectSingleElementAsync(this PageTest pageTest, ILocator parent, string expression)
    {
        var element = parent.Locator(expression);
        await pageTest.Expect(element).ToHaveCountAsync(1);
        await pageTest.Expect(element).ToBeVisibleAsync();

        return element;
    }

    public static async Task<ILocator> ExpectSingleElementAsync(this PageTest pageTest, IPage page, string expression)
    {
        var element = page.Locator(expression);
        await pageTest.Expect(element).ToHaveCountAsync(1);
        await pageTest.Expect(element).ToBeVisibleAsync();

        return element;
    }

    public static async Task FillOrClearAsync(this ILocator locator, string? value)
    {
        if (!string.IsNullOrEmpty(value))
            await locator.FillAsync(value);
        else
            await locator.ClearAsync();
    }

    public static async Task<ILocator> ExpectConfirmationLocatorAsync(this PageTest pageTest)
    {
        var confirmationLocator = pageTest.Page.Locator(".jconfirm-box"); //.Locator("div[class='jconfirm-box jconfirm-hilight-shake jconfirm-type-default jconfirm-type-animated']");
        await pageTest.Expect(confirmationLocator).ToHaveCountAsync(1);
        await pageTest.Expect(confirmationLocator).ToBeVisibleAsync();        

        return confirmationLocator;
    }

    public static async Task<ILocator> ExpectConfirmationButtonAsync(this PageTest pageTest, ILocator confirmationLocator, string buttonText)
    {
        var buttonLocator = confirmationLocator.Locator("div[class='jconfirm-buttons']").GetByText(buttonText);
        await pageTest.Expect(buttonLocator).ToHaveCountAsync(1);
        return buttonLocator;
    }

    public static async Task<ILocator> ExpectConfirmationYesButtonAsync(this PageTest pageTest, ILocator confirmationLocator)
    {
        return await pageTest.ExpectConfirmationButtonAsync(confirmationLocator, "Yes");
    }

    public static async Task<ILocator> ExpectConfirmationCancelButtonAsync(this PageTest pageTest, ILocator confirmationLocator)
    {
        return await pageTest.ExpectConfirmationButtonAsync(confirmationLocator, "Cancel");
    }

    public static async Task<ILocator> ExpectElementByAriaRoleAndTextAsync(this PageTest pageTest, AriaRole ariaRole, string text)
    {
        var elementLocator = pageTest.Page.GetByRole(ariaRole).GetByText(text);
        await pageTest.Expect(elementLocator).ToHaveCountAsync(1);
        await pageTest.Expect(elementLocator).ToBeVisibleAsync();
        return elementLocator;
    }

    public static async Task ExpectFileUploadAsync(this PageTest pageTest, string className, string fileName, string filePath)
    {
        var fileUpload = pageTest.Page.Locator($"file-upload.{className}");
        await pageTest.Expect(pageTest.Page.GetByText(fileName)).Not.ToBeVisibleAsync();
        await fileUpload.Locator("button").ClickAsync();
        await fileUpload.Locator("input").SetInputFilesAsync(filePath);
        await pageTest.Expect(pageTest.Page.Locator($"file-upload.{className}").Locator(".file-info")).ToHaveTextAsync(fileName);
    }
}