using Alchemist.Product.Entities;
using Alchemist.Product.Import.WebApp.Test.Infrastructure;
using Alchemist.Product.Interfaces;
using Moq;
using Xunit.Abstractions;

namespace Alchemist.Product.Import.WebApp.Test;

public class ShopActionsTest : ImportWebAppTest
{
    public ShopActionsTest(TestImportWebAppFactory webAppFactory, ITestOutputHelper testOutputHelper) 
        : base(webAppFactory, testOutputHelper, httpPort:8112, httpsPort:8113)
    {
        _webAppFactory.ShopAPIClient.Setup(s => s.CreateShop(It.IsAny<IShop>())).Returns((IShop shop) =>
        {
            shop.Id = _shops.Count + 1;
            _shops.Add(shop);
            return Task.FromResult(shop);
        });
        
        _webAppFactory.ShopAPIClient.Setup(s => s.UpdateShop(It.IsAny<IShop>())).Returns((IShop shop) =>
        {
            var existingShop = _shops.FirstOrDefault(s => s.Id == shop.Id) ?? throw new ArgumentException();
            existingShop.Name = shop.Name;
            existingShop.Url = shop.Url;
            existingShop.Caption = shop.Caption;
            
            return Task.FromResult(existingShop);
        });
    }

    [Fact]
    public async Task CreateNewShop()
    {
        var startShopImportSettings = await ExpectLoadIndexPageAsync(Page);

        var shopFormLocator = Page.Locator("#shopForm");
        await Expect(shopFormLocator).ToBeHiddenAsync();
        
        await Page.Locator("#createNewShop").ClickAsync();

        await Expect(shopFormLocator).ToBeVisibleAsync();

        var newShop = new Shop()
        {
            Name = Guid.NewGuid().ToString(),
            Url = Guid.NewGuid().ToString(),
            Caption = Guid.NewGuid().ToString(),
        };

        await shopFormLocator.Locator("#Name").FillAsync(newShop.Name);
        await shopFormLocator.Locator("#Url").FillAsync(newShop.Url);
        await shopFormLocator.Locator("#Caption").FillAsync(newShop.Caption);

        await shopFormLocator.GetByRole(Microsoft.Playwright.AriaRole.Button).ClickAsync();

        await Expect(shopFormLocator).ToBeHiddenAsync();

        await Expect(Page.Locator(".shopsList").GetByText(newShop.Name)).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task EditShop()
    {
        var startShopImportSettings = await ExpectLoadIndexPageAsync(Page);

        var editShopLocator = await this.ExpectSingleElementAsync(Page,"i[class='fa fa-edit']");
        await editShopLocator.ClickAsync();

        var shopFormLocator = await this.ExpectSingleElementAsync(Page,"#shopForm");       

        var shop = _shops.FirstOrDefault(s => s.Id == startShopImportSettings.ShopId);
        var copy = shop.To<Shop>();

        await Expect(shopFormLocator.Locator("#Name")).ToHaveValueAsync(shop.Name);
        await Expect(shopFormLocator.Locator("#Url")).ToHaveValueAsync(shop.Url);
        if(string.IsNullOrEmpty(shop.Caption))
            await Expect(shopFormLocator.Locator("#Caption")).ToBeEmptyAsync();
        else
            await Expect(shopFormLocator.Locator("#Caption")).ToHaveValueAsync(shop.Caption);

        shop.Caption = Guid.NewGuid().ToString();
        shop.Url = Guid.NewGuid().ToString();
        shop.Name = Guid.NewGuid().ToString();

        await shopFormLocator.Locator("#Name").FillAsync(shop.Name);
        await shopFormLocator.Locator("#Url").FillAsync(shop.Url);
        await shopFormLocator.Locator("#Caption").FillAsync(shop.Caption);

        await shopFormLocator.GetByRole(Microsoft.Playwright.AriaRole.Button).ClickAsync();

        await Expect(shopFormLocator).ToBeHiddenAsync();

        await Expect(Page.Locator(".shopsList").GetByText(shop.Name)).ToHaveCountAsync(1);
        await Expect(Page.Locator(".shopsList").GetByText(copy.Name)).ToHaveCountAsync(0);
    }
}
