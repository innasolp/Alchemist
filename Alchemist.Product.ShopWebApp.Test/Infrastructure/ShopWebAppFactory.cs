using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shop.API.Client;
using Shop.Interfaces;

namespace Alchemist.Product.ShopWebApp.Test.Infrastructure;

public class ShopWebAppFactory : TestWebAppKestrelFactory<ShopWebAppProgram>
{
    public ShopWebAppFactory() : base(8410, 8411)
    {
    }

    private readonly Mock<IShopDataService> _shopAPIClient = new();    

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        services.InterceptImplementation<IShopDataService, ShopApiClient>(_shopAPIClient.Object);
    }

    public void SetupShops()
    {
        _shopAPIClient.Reset();
        var shops = TestRepository.GetShopsTestData(new Random().Next(2, 10)).OfType<IShop>().ToList();
        int i = 0;
        foreach (IShop shop in shops)
        {
            i++;
            shop.Id = i;
        }

        _shopAPIClient.Setup(s => s.GetShops(It.IsAny<CancellationToken>())).Returns(Task.FromResult(shops));
        _shopAPIClient.Setup(s => s.GetShop(It.IsAny<int>(), It.IsAny<CancellationToken>())).
            Returns((int id, CancellationToken token) => Task.FromResult(shops.FirstOrDefault(s => s.Id == id)));
        _shopAPIClient.Setup(s => s.CreateShop(It.IsAny<IShop>(), It.IsAny<CancellationToken>())).ReturnsAsync((IShop shop, CancellationToken token) =>
        {
            shop.Id = shops.Count + 1;
            shops.Add(shop);
            return shop;
        });
        _shopAPIClient.Setup(s => s.UpdateShop(It.IsAny<IShop>(), It.IsAny<CancellationToken>())).ReturnsAsync((IShop shop, CancellationToken token) =>
        {
            var currentShop = shops.FirstOrDefault(s => s.Id == shop.Id);
            if (currentShop == null) return default;
            currentShop.Name = shop.Name;
            currentShop.Url = shop.Url;
            currentShop.Caption = shop.Caption;
            return currentShop;
        });
    }

    public async Task<List<IShop>> GetShopsAsync()
    {
        return await _shopAPIClient.Object.GetShops();
    }
}
