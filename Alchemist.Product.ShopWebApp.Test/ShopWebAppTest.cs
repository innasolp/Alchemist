using Alchemist.Product.Interfaces;
using Alchemist.Product.ShopWebApp.Test.Infrastructure;
using Alchemist.Test.Functional.Playwright;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Xunit.Abstractions;
using Shop = Alchemist.Product.Entities.Shop;

namespace Alchemist.Product.ShopWebApp.Test;

public class ShopWebAppTest(ShopWebAppFactory shopWebAppFactory, ITestOutputHelper testOutputHelper) : PageTest, IClassFixture<ShopWebAppFactory>
{
    private readonly ShopWebAppFactory _webAppFactory = shopWebAppFactory;

    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;    

    private async Task ExpectLoadShopListAsync(int shopsCount)
    {
        var response = await Page.GotoAsync(_webAppFactory.ServerAddress);
        Assert.True(response?.Ok);

        await Expect(Page.Locator("a.shop_item")).ToHaveCountAsync(shopsCount);
    }

    private async Task ExpectLoadShopAsync()
    {   
        await Expect(Page.Locator("#Name")).Not.ToBeEmptyAsync();
    }

    private async Task ExpectShopEditFormFilledByShopFieldValuesAsync(IShop shop)
    {
        var shopEditFormLocator = Page.Locator("#shopEditForm");
        await Expect(shopEditFormLocator).ToHaveCountAsync(1);

        await this.ExpectWithNullValueAsync(shopEditFormLocator.Locator("#Name"), shop.Name);
        await this.ExpectWithNullValueAsync(shopEditFormLocator.Locator("#Url"), shop.Url);
        await this.ExpectWithNullValueAsync(shopEditFormLocator.Locator("#Caption"), shop.Caption);
    }

    private async Task FillShopEditFormFieldsAsync(IShop shop)
    {
        ArgumentNullException.ThrowIfNull(shop);
        var shopEditFormLocator = Page.Locator("#shopEditForm");
        await Expect(shopEditFormLocator).ToHaveCountAsync(1);

        await shopEditFormLocator.Locator("#Name").FillOrClearAsync(shop.Name);
        await shopEditFormLocator.Locator("#Url").FillOrClearAsync(shop.Url);
        await shopEditFormLocator.Locator("#Caption").FillOrClearAsync(shop.Caption);
    }

    private async Task GoToShopAsync(int shopId)
    {
        var url = $"{_webAppFactory.ServerAddress}Shop/{shopId}";
        var response = await Page.GotoAsync(url);
        Assert.True(response?.Ok);
    }

    private async Task SelectShopAsync(int shopId)
    {
        var shopItemLocator = Page.Locator($"a[href='/Shop/{shopId}']");
        await Expect(shopItemLocator).ToHaveCountAsync(1);
        await shopItemLocator.ClickAsync();
    }

    private static void SetRandomValues(IShop shop)
    {
        shop.Name = Guid.NewGuid().ToString();
        shop.Url = Guid.NewGuid().ToString();
        shop.Caption = Guid.NewGuid().ToString();
    }

    private static IShop GetRandomShop(IEnumerable<IShop> shops, int currentShopId)
    {
        var nextShopId = new Random().Next(1, shops.Count() + 1);
        while (nextShopId == currentShopId)
            nextShopId = new Random().Next(1, shops.Count() + 1);
        return shops.First(s => s.Id == nextShopId);
    }

    private async Task ExpectSelectedShopItemOnShopListAsync(IShop shop)
    {
        var selectedShopLocator = Page.Locator("a[class='shop_item selected']");
        await Expect(selectedShopLocator).ToHaveCountAsync(1);
        await Expect(selectedShopLocator).ToHaveTextAsync(shop.Name);
    }

    private async Task GoToNewShopAsync()
    {
        var newShopLocator = Page.Locator("#createNewShop");
        await newShopLocator.ClickAsync();
    }

    private async Task ExpectSaveClickAsync()
    {
        var saveButton = await this.ExpectElementByAriaRoleAndTextAsync(AriaRole.Button, "Save");
        await saveButton.ClickAsync();
    }

    private async Task<List<IShop>> ExpectInitializeAsync()
    {
        _webAppFactory.SetupShops();
        var shops = await _webAppFactory.GetShopsAsync();

        await ExpectLoadShopListAsync(shops.Count);

        return shops;
    }

    [Fact]
    public async Task IndexPageLoadShopListAndFirstShopByDefaultAsync()
    {
        var shops = await ExpectInitializeAsync();

        var defaultShop = shops.First();

        await ExpectSelectedShopItemOnShopListAsync(defaultShop);
    }

    [Fact]
    public async Task ShopItemIsSelectedOnUrlIndexByShopIdAsync()
    {
        var shops = await ExpectInitializeAsync();

        var shop = shops[new Random().Next(0, shops.Count)];
        await GoToShopAsync(shop.Id);

        await ExpectSelectedShopItemOnShopListAsync(shop);  
        await ExpectShopEditFormFilledByShopFieldValuesAsync(shop);
    }

    [Fact]
    public async Task EmptyFieldsAndNoSelectionOnShopNewAsync()
    {
        await ExpectInitializeAsync();

        await GoToNewShopAsync();

        var selectedShopLocator = Page.Locator("a[class='shop_item selected']");
        await Expect(selectedShopLocator).ToHaveCountAsync(0);

        var newShop = new Shop();
        await ExpectShopEditFormFilledByShopFieldValuesAsync(newShop);
    }

    private async Task<ILocator> ExpectConfirmationOnNextShopSelectingWhenPreviousEditedWithoutSavingAsync(IShop editedShop, int nextShopId)
    {
        await FillShopEditFormFieldsAsync(editedShop);   
        
        await SelectShopAsync(nextShopId);

        return await this.ExpectConfirmationLocatorAsync();
    }

    [Fact]
    public async Task ConfirmationAboutChangesOnEditShopFieldsAsync()
    {
        var shops = await ExpectInitializeAsync();

        var shop = shops[new Random().Next(0, shops.Count)];
        await GoToShopAsync(shop.Id);

        var editedShop = new Shop();
        SetRandomValues(editedShop);

        var nextShop = GetRandomShop(shops, shop.Id);

        await ExpectConfirmationOnNextShopSelectingWhenPreviousEditedWithoutSavingAsync(editedShop, nextShop.Id);
    }

    [Fact]
    public async Task InputValuesNotSavedWhenConfirmationResetedAsync()
    {
        var shops = await ExpectInitializeAsync();

        var shop = shops[new Random().Next(0, shops.Count)];
        await GoToShopAsync(shop.Id);

        var nextShop = GetRandomShop(shops, shop.Id);

        var editedShop = new Shop();
        SetRandomValues(editedShop);

        var confirmation = await ExpectConfirmationOnNextShopSelectingWhenPreviousEditedWithoutSavingAsync(editedShop, nextShop.Id);
        var yesButton = await this.ExpectConfirmationYesButtonAsync(confirmation);
        await yesButton.ClickAsync();
        
        await ExpectSelectedShopItemOnShopListAsync(nextShop);
        await ExpectShopEditFormFilledByShopFieldValuesAsync(nextShop);

        await SelectShopAsync(shop.Id);

        await ExpectSelectedShopItemOnShopListAsync(shop);
        await ExpectShopEditFormFilledByShopFieldValuesAsync(shop);
    }

    [Fact]
    public async Task NextShopNotSelectedWhenResetingConfirmationPreviousShopCancelledAsync()
    {
        var shops = await ExpectInitializeAsync();

        var shop = shops[new Random().Next(0, shops.Count)];
        await GoToShopAsync(shop.Id);

        var nextShop = GetRandomShop(shops, shop.Id);

        var editedShop = new Shop();
        SetRandomValues(editedShop);

        var confirmation = await ExpectConfirmationOnNextShopSelectingWhenPreviousEditedWithoutSavingAsync(editedShop, nextShop.Id);
        var cancelButton = await this.ExpectConfirmationCancelButtonAsync(confirmation);
        await cancelButton.ClickAsync();

        await ExpectSelectedShopItemOnShopListAsync(shop);
        await ExpectShopEditFormFilledByShopFieldValuesAsync(editedShop);
    }

    [Fact]
    public async Task ShopItemAndFieldsUpdatedWhenShopEditedAndSavedAsync()
    {
        var shops = await ExpectInitializeAsync();

        await ExpectLoadShopAsync();

        var shop = shops[new Random().Next(0, shops.Count)];
        await GoToShopAsync(shop.Id);

        var editedShop = new Shop();
        SetRandomValues(editedShop);

        await FillShopEditFormFieldsAsync(editedShop);

        await ExpectSaveClickAsync();

        var nextShop = GetRandomShop(shops, shop.Id);
        await SelectShopAsync(nextShop.Id);        

        await SelectShopAsync(shop.Id);

        await ExpectSelectedShopItemOnShopListAsync(editedShop);
        await ExpectShopEditFormFilledByShopFieldValuesAsync(editedShop);
    }

    [Fact]
    public async Task NewShopItemAddedToListWhenNewShopSavedAsync()
    {
        await ExpectInitializeAsync();

        await GoToNewShopAsync();

        var newShop = new Shop();
        SetRandomValues(newShop);
        await FillShopEditFormFieldsAsync(newShop);

        await ExpectSaveClickAsync();

        await ExpectSelectedShopItemOnShopListAsync(newShop);
    }

    [Fact]
    public async Task ShopNotSaveWhenValidationFailedAsync()
    {
        var shops = await ExpectInitializeAsync();

        var shop = shops[new Random().Next(0, shops.Count)];
        await GoToShopAsync(shop.Id);

        var editedShop = new Shop();
        await FillShopEditFormFieldsAsync(editedShop);

        await ExpectSaveClickAsync();

        await Expect(Page.Locator("#Name-error")).ToBeVisibleAsync();
        await Expect(Page.Locator("#Url-error")).ToBeVisibleAsync();

        await GoToShopAsync(shop.Id);

        await ExpectShopEditFormFilledByShopFieldValuesAsync(shop);
    }
}