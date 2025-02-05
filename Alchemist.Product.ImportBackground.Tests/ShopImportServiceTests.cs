using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Products.Service;
using Alchemist.Product.Import.Background;
using Alchemist.Product.Interfaces;
using Message.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Alchemist.Product.ImportBackground.Tests;

public class ShopImportServiceTests
{
    private readonly string _shopProductsJsonFile = "shopProducts.json";
    private readonly string _shopCategoriesJsonFile = "shopCategories.json";

    private readonly ShopCategoriesSettings[] _shopCategoriesSettings;
    private readonly ShopProductsSettings[] _shopProductsSettings;

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

        _shopCategoriesSettings = _shopCategoriesJsonFile.GetSettings<ShopCategoriesSettings[]>("ShopCategories");
        _shopProductsSettings = _shopProductsJsonFile.GetSettings<ShopProductsSettings[]>("ShopProducts");
    }

    private void AddBaseServices()
    {
        _builder.Services.AddSingleton(_shopApiClientMock.Object);
        _builder.Services.AddSingleton(_alchemyDataServiceMock.Object);
        _builder.Services.AddKeyedSingleton(ShopImportWorkerKeys.DataMessageReceiverKey, _messageReceiver.Object);
        _builder.Services.AddKeyedSingleton(ShopImportWorkerKeys.ShopsMessageSenderKey, _messageSender.Object);
    }


    [Fact]
    public void BuildShopImportServiceWithProductsAndCategoriesTest()
    {
        AddBaseServices();

        _shopImportWorkerBuilder.AddShopProducts(_shopProductsSettings, Utils.GetAppPath());
        _shopImportWorkerBuilder.AddShopCategories(_shopCategoriesSettings, Utils.GetAppPath());

        _shopImportWorkerBuilder.AddProductsHandler();
        _shopImportWorkerBuilder.AddCategoriesHandler();

        _shopImportWorkerBuilder.AddPropertyValueInterceptorsLogging(_shopCategoriesSettings, "ClassName", (shopSetting) => shopSetting.Id);

        _shopImportWorkerBuilder.AddScoped<ShopImportWorker>();

        _builder.Services.AddHttpClient();

        var host = _builder.Build();

        var shopImportService = host.Services.GetService<ShopImportWorker>();

        Assert.NotNull(shopImportService);

        Assert.Equal(2, host.Services.GetServices<IShopCategoryImportService>().Count());
        Assert.Equal(2, host.Services.GetServices<IShopProductImportService>().Count());
    }

    [Fact]
    public void BuildShopImportServiceWithProductsOnlyTest()
    {
        AddBaseServices();

        _shopImportWorkerBuilder.AddShopProducts(_shopProductsSettings, Utils.GetAppPath());

        _shopImportWorkerBuilder.AddProductsHandler();

        _shopImportWorkerBuilder.AddScoped<ShopImportWorker>();

        _builder.Services.AddHttpClient();

        var host = _builder.Build();

        var shopImportService = host.Services.GetService<ShopImportWorker>();

        Assert.NotNull(shopImportService);

        Assert.Empty(host.Services.GetServices<IShopCategoryImportService>());
        Assert.Equal(2, host.Services.GetServices<IShopProductImportService>().Count());
    }
}