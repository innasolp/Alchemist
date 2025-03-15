using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Products.Service;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Background;
using Message.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Alchemist.Product.ImportBackground.Tests;

public abstract class BuildImportServiceTest
{
    protected List<IShopImportData> _shopImportData;

    protected readonly HostApplicationBuilder _builder;

    protected readonly ShopImportWorkerBuilder _shopImportWorkerBuilder;

    private readonly IMock<IShopDataService> _shopApiClientMock = new Mock<IShopDataService>();

    private readonly IMock<IProductDataService> _alchemyDataServiceMock = new Mock<IProductDataService>();
    private readonly IMock<IMessageReceiver> _messageReceiver = new Mock<IMessageReceiver>();
    private readonly IMock<IMessageSender> _messageSender = new Mock<IMessageSender>();

    public BuildImportServiceTest()
    {
        _builder = new HostApplicationBuilder();
        _shopImportWorkerBuilder = new ShopImportWorkerBuilder(_builder);
        _builder.Services.AddSingleton(_shopApiClientMock.Object);
        _builder.Services.AddSingleton(_alchemyDataServiceMock.Object);
        _builder.Services.AddKeyedSingleton(ShopImportWorkerKeys.DataMessageReceiverKey, _messageReceiver.Object);
        _builder.Services.AddKeyedSingleton(ShopImportWorkerKeys.ShopsMessageSenderKey, _messageSender.Object);
    }

    protected abstract Task SetShopSettings();

    [Fact]
    public async Task BuildShopImportServiceWithProductsAndCategoriesTestAsync()
    {
        await SetShopSettings();

        var shopProductImportSettings = _shopImportData.Select(s => s.ProductShopImportSettings).ToList();
        var shopCategoryImportSettings = _shopImportData.Select(s => s.CategoryShopImportSettings).ToList();

        foreach (var shop in shopProductImportSettings)
            shop.SetAppPath(Utils.GetAppPath());

        foreach (var shop in shopCategoryImportSettings)
            shop.SetAppPath(Utils.GetAppPath());

        _shopImportWorkerBuilder.AddShopProducts(_shopImportData);
        _shopImportWorkerBuilder.AddShopCategories(_shopImportData);

        _shopImportWorkerBuilder.AddProductsHandler();
        _shopImportWorkerBuilder.AddCategoriesHandler();

        _shopImportWorkerBuilder.AddPropertyValueInterceptorsLogging(shopCategoryImportSettings, "ClassName", (shopSetting) => shopSetting.Id);

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
        await SetShopSettings();

        var shopProductImportSettings = _shopImportData.Select(s => s.ProductShopImportSettings).ToList();

        foreach (var shop in shopProductImportSettings)
            shop.SetAppPath(Utils.GetAppPath());

        _shopImportWorkerBuilder.AddShopProducts(_shopImportData);

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
