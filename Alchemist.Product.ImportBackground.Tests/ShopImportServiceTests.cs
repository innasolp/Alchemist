using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Products.Service;
using Alchemist.Import.Settings.Model;
using Alchemist.Product.Import.Background;
using Alchemist.Product.Interfaces;
using Json.Extensions;
using Message.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Alchemist.Product.ImportBackground.Tests;

public class ShopImportServiceTests
{
    private readonly string _shopProductsJsonFile = "shopProducts.json";
    private readonly string _shopCategoriesJsonFile = "shopCategories.json";

    private ShopImportSettings[] _shopCategoriesSettings;
    private ProductShopImportSettings[] _shopProductsSettings;

    private readonly HostApplicationBuilder _builder;

    private readonly ShopImportWorkerBuilder _shopImportWorkerBuilder;

    private readonly IMock<IShopDataService> _shopApiClientMock = new Mock<IShopDataService>();

    private readonly IMock<IProductDataService> _alchemyDataServiceMock = new Mock<IProductDataService>();
    private readonly IMock<IMessageReceiver> _messageReceiver = new Mock<IMessageReceiver>();
    private readonly IMock<IMessageSender> _messageSender = new Mock<IMessageSender>();

    public ShopImportServiceTests()
    {
        _builder = new HostApplicationBuilder();
        _shopImportWorkerBuilder = new ShopImportWorkerBuilder(_builder);
    }

    private async Task AddBaseServicesAsync()
    {
        _shopCategoriesSettings = await _shopCategoriesJsonFile.ReadFromJsonFileAsync<ShopImportSettings[]>();// "ShopCategories");
        _shopProductsSettings = await _shopProductsJsonFile.ReadFromJsonFileAsync<ProductShopImportSettings[]>();// "ShopProducts");

        _builder.Services.AddSingleton(_shopApiClientMock.Object);
        _builder.Services.AddSingleton(_alchemyDataServiceMock.Object);
        _builder.Services.AddKeyedSingleton(ShopImportWorkerKeys.DataMessageReceiverKey, _messageReceiver.Object);
        _builder.Services.AddKeyedSingleton(ShopImportWorkerKeys.ShopsMessageSenderKey, _messageSender.Object);
    }


    [Fact]
    public async Task BuildShopImportServiceWithProductsAndCategoriesTestAsync()
    {
        await AddBaseServicesAsync();

        foreach (var shop in _shopProductsSettings)
            shop.SetAppPath(Utils.GetAppPath());
        
        foreach (var shop in _shopCategoriesSettings)
            shop.SetAppPath(Utils.GetAppPath());

        _shopImportWorkerBuilder.AddShopProducts(_shopProductsSettings);
        _shopImportWorkerBuilder.AddShopCategories(_shopCategoriesSettings);

        _shopImportWorkerBuilder.AddProductsHandler();
        _shopImportWorkerBuilder.AddCategoriesHandler();

        _shopImportWorkerBuilder.AddPropertyValueInterceptorsLogging(_shopCategoriesSettings, "ClassName", (shopSetting) => shopSetting.Id);

        _builder.Services.AddHttpClient();

        _builder.Services.AddScoped<ShopImportWorker>();

        
        var host = _builder.Build();

        var shopImportService = host.Services.GetService<ShopImportWorker>();

        Assert.NotNull(shopImportService);

        Assert.Equal(2, host.Services.GetServices<IShopCategoryImportService>().Count());
        Assert.Equal(2, host.Services.GetServices<IShopProductImportService>().Count());
    }

    [Fact]
    public async Task BuildShopImportServiceWithProductsOnlyTestAsync()
    {
        await AddBaseServicesAsync();

        foreach (var shop in _shopProductsSettings)
            shop.SetAppPath(Utils.GetAppPath());

        _shopImportWorkerBuilder.AddShopProducts(_shopProductsSettings);

        _shopImportWorkerBuilder.AddProductsHandler();

        _builder.Services.AddScoped<ShopImportWorker>();

        _builder.Services.AddHttpClient();

        var host = _builder.Build();

        var shopImportService = host.Services.GetService<ShopImportWorker>();

        Assert.NotNull(shopImportService);

        Assert.Empty(host.Services.GetServices<IShopCategoryImportService>());
        Assert.Equal(2, host.Services.GetServices<IShopProductImportService>().Count());
    }
}