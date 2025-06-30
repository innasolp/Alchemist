using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.WebApp.Models;
using Microsoft.Playwright;
using System.Text.Json;
using Xunit.Abstractions;

namespace Alchemist.Product.Import.WebApp.Test;

public class ProductShopSettingsRootCategoryActionTest(TestImportWebAppFactory webAppFactory, ITestOutputHelper testOutputHelper)
    : ImportWebAppTest(webAppFactory, testOutputHelper, httpPort: 8116, httpsPort: 8117)
{
    private async Task<ILocator> ExpectAddCategoryFormModalAsync(IPage page)
    {
        var divCategoryModal = page.Locator("#divRootCategoryModal");
        await Expect(divCategoryModal).Not.ToBeVisibleAsync();

        var settingsForm = page.Locator("#settingsForm");

        var addCategoryButton = settingsForm.Locator("#addRootCategory");
        await Expect(addCategoryButton).ToBeVisibleAsync();

        var rootCategoryForm = settingsForm.Locator("#rootCategoryForm");
        await Expect(rootCategoryForm).Not.ToBeVisibleAsync();

        await addCategoryButton.ClickAsync();
        await Expect(rootCategoryForm).ToBeVisibleAsync();

        await Expect(divCategoryModal).ToBeVisibleAsync();

        return rootCategoryForm;
    }

    private async Task<ILocator?> ExpectSetCategoryFormModalAsync(ILocator settingsForm, ICategoryUrl categoryUrl)
    {
        var divCategoryModal = settingsForm.Locator("#divRootCategoryModal");
        await Expect(divCategoryModal).Not.ToBeVisibleAsync();

        var rootCategoryForm = settingsForm.Locator("#rootCategoryForm");
        await Expect(rootCategoryForm).Not.ToBeVisibleAsync();

        var rootCategoriesTable = settingsForm.Locator("#rootCategoriesUl");
        await Expect(rootCategoriesTable).ToBeVisibleAsync();

        var rows = await rootCategoriesTable.Locator(".rootCategoryRow").AllAsync();
        ILocator? categoryRow = null;
        foreach (var row in rows)
        {
            if (await row.Locator(".item").TextContentAsync() == categoryUrl.Item.ToString()
                && await row.Locator(".url").TextContentAsync() == categoryUrl.Url)
            {
                categoryRow = row;
                break;
            }
        }

        if (categoryRow == null) return null;

        await Expect(categoryRow).ToHaveCountAsync(1);

        var buttonEdit = categoryRow.Locator(".editRootCategory");
        await Expect(buttonEdit).ToHaveCountAsync(1);

        await buttonEdit.ClickAsync();

        await Expect(rootCategoryForm).ToBeVisibleAsync();

        await Expect(divCategoryModal).ToBeVisibleAsync();

        return rootCategoryForm;
    }

    private async Task FillRootCategoryFormFieldsAsync(ILocator rootCategoryForm, ICategoryUrl rootCategory)
    {
        var itemLocator = rootCategoryForm.Locator(".rootCategoryItem");
        await itemLocator.FillAsync(rootCategory.Item.ToString());

        var urlLocator = rootCategoryForm.Locator(".rootCategoryUrl");
        await urlLocator.FillAsync(rootCategory.Url);

        var setButton = rootCategoryForm.Locator("button");
        await Expect(setButton).ToBeVisibleAsync();

        await setButton.ClickAsync();
    }

    private async Task ExpectRowsInRootCategoriesTableAsync(ILocator settingsForm, ICategoryUrl[] categories)
    {
        var rootCategoriesTable = settingsForm.Locator("#rootCategoriesUl");
        await Expect(rootCategoriesTable).ToBeVisibleAsync();

        var rootCategoriesRows = settingsForm.Locator(".rootcategory_li");
        await Expect(rootCategoriesRows).ToHaveCountAsync(categories.Length);

        var rows = (await rootCategoriesRows.AllAsync()).ToArray();
        for (var i = 0; i < rows.Length; i++)
        {
            var item = rows[i].Locator(".item");
            await Expect(item).ToHaveTextAsync(categories[i].Item.ToString());

            var url = rows[i].Locator(".url");
            await Expect(url).ToHaveTextAsync(categories[i].Url);
        }
    }
       

    [Fact]
    public async Task RootCategoryModalWhenAddRootCategoryButtonClick()
    {
        var newPage = await Context.NewPageAsync();

        await ExpectLoadIndexPageAsync(newPage);

        await ExpectAddCategoryFormModalAsync(newPage);
    }

    [Fact]
    public async Task RootCategoriesTableContainsRowsWhenRootCategoriesExists()
    {
        var shopSetting = _shopSettings.First();
        var productShopImportSettings = new ProductShopSettingsModel()
        {
            Name = shopSetting.Name,
            ProductUrlFormat = $"https://url{shopSetting.Id}_product",
            CategoryUrlFormat = $"https://url{shopSetting.Id}_category"                
        };

        productShopImportSettings.RootCategories.AddRange([
                    new CategoryUrlModel (productShopImportSettings.Guid)  { Item = 6500, Url = $"https://rootcategory_6500_{shopSetting.Id}" },
                    new CategoryUrlModel (productShopImportSettings.Guid){ Item = 6501, Url = $"https://rootcategory_6501_{shopSetting.Id}" },
                ]);
        shopSetting.JsonValue = JsonSerializer.Serialize(productShopImportSettings);

        var newPage = await Context.NewPageAsync();

        await ExpectLoadIndexPageAsync(newPage);

        var settingsForm = newPage.Locator("#settingsForm");

        await ExpectRowsInRootCategoriesTableAsync(settingsForm, productShopImportSettings.RootCategories.ToArray());
    }

    [Fact]
    public async Task ValidationRootCategoryFailedWhenAnyRequiredFieldsNotFill()
    {
        var newPage = await Context.NewPageAsync();

        await ExpectLoadIndexPageAsync(newPage);

        var rootCategoryForm = await ExpectAddCategoryFormModalAsync(newPage);

        var itemLocator = rootCategoryForm.Locator(".rootCategoryItem");
        await itemLocator.FillAsync("-2");

        var setButton = rootCategoryForm.Locator("button");
        await Expect(setButton).ToBeVisibleAsync();

        await setButton.ClickAsync();

        var divCategoryModal = newPage.Locator("#divRootCategoryModal");
        await Expect(divCategoryModal).ToBeVisibleAsync();

        await Expect(rootCategoryForm.Locator("#Item-error")).ToBeVisibleAsync();
        await Expect(rootCategoryForm.Locator("#Item-error")).ToContainTextAsync("must be between");

        await Expect(rootCategoryForm.Locator("#Url-error")).ToBeVisibleAsync();
        await Expect(rootCategoryForm.Locator("#Url-error")).ToHaveTextAsync("The Url field is required.");
    }

    [Fact]
    public async Task RowAddToRootCategoriesTableWhenNewRootCategoryAdded()
    {
        await Context.ClearCookiesAsync();

        
        var newPage = await Context.NewPageAsync();

        var shopSettings = await ExpectLoadIndexPageAsync(newPage);

        var rootCategory = new CategoryUrlModel(shopSettings.Guid) { Item = 6500, Url = "https://category_6500" };
        var rootCategoryForm = await ExpectAddCategoryFormModalAsync(newPage);

        await FillRootCategoryFormFieldsAsync(rootCategoryForm, rootCategory);

        await Expect(rootCategoryForm.Locator("#Item-error")).Not.ToBeVisibleAsync();
        await Expect(rootCategoryForm.Locator("#Url-error")).Not.ToBeVisibleAsync();

        var divCategoryModal = newPage.Locator("#divRootCategoryModal");
        await Expect(divCategoryModal).Not.ToBeVisibleAsync();

        var settingsForm = newPage.Locator("#settingsForm");
        await ExpectRowsInRootCategoriesTableAsync(settingsForm, [rootCategory]);
    }

    [Fact]
    public async Task RootCategoryCellChangedWhenRootCategoryEdited()
    {
        var shopSetting = _shopSettings.First();
        var productShopImportSettings = new ProductShopSettingsModel()
        {
            Name = shopSetting.Name,
            ProductUrlFormat = $"https://url{shopSetting.Id}_product",
            CategoryUrlFormat = $"https://url{shopSetting.Id}_category"
        };
        productShopImportSettings.RootCategories.AddRange([
                    new CategoryUrlModel (productShopImportSettings.Guid)  { Item = 6500, Url = $"https://rootcategory_6500_{shopSetting.Id}" },
                    new CategoryUrlModel (productShopImportSettings.Guid){ Item = 6501, Url = $"https://rootcategory_6501_{shopSetting.Id}" },
                ]);
        shopSetting.JsonValue = JsonSerializer.Serialize(productShopImportSettings);

        var newPage = await Context.NewPageAsync();

        await ExpectLoadIndexPageAsync(newPage);

        var settingsForm = newPage.Locator("#settingsForm");

        var rootCategoryForm = await ExpectSetCategoryFormModalAsync(settingsForm, productShopImportSettings.RootCategories[0]);
        Assert.NotNull(rootCategoryForm);

        var divCategoryModal = newPage.Locator("#divRootCategoryModal");
        await Expect(divCategoryModal).ToBeVisibleAsync();

        var rootCategory = new CategoryUrlModel(productShopImportSettings.Guid) { Item = 6502, Url = $"https://rootcategory_6502" };

        await FillRootCategoryFormFieldsAsync(rootCategoryForm, rootCategory);

        await Expect(divCategoryModal).Not.ToBeVisibleAsync();

        await ExpectRowsInRootCategoriesTableAsync(settingsForm, [rootCategory, productShopImportSettings.RootCategories[1]]);
    }
}
