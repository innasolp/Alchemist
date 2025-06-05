using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.Model;
using Alchemist.Product.Import.Background;
using Message.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Alchemist.Product.ImportBackground.Tests;

public abstract class BuildImportServiceTest
{
    protected ICategoryShopImportSettings[] _shopCategoriesSettings;
    protected IProductShopImportSettings[] _shopProductsSettings;

    protected readonly HostApplicationBuilder _builder;

    private readonly IMock<IShopDataService> _shopApiClientMock = new Mock<IShopDataService>();

    private readonly IMock<IProductDataService> _alchemyDataServiceMock = new Mock<IProductDataService>();
    private readonly IMock<IMessageReceiver> _messageReceiver = new Mock<IMessageReceiver>();
    private readonly IMock<IMessageSender> _messageSender = new Mock<IMessageSender>();

    public BuildImportServiceTest()
    {
        _builder = new HostApplicationBuilder();        
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

        foreach (var shop in _shopProductsSettings)
            shop.SetAppPath(Utils.GetAppPath());

        foreach (var shop in _shopCategoriesSettings)
            shop.SetAppPath(Utils.GetAppPath());

        //todo
        //_shopImportWorkerBuilder.AddShopProducts(_shopProductsSettings);
        //_shopImportWorkerBuilder.AddShopCategories(_shopCategoriesSettings);

        //_shopImportWorkerBuilder.AddProductsHandler();
        //_shopImportWorkerBuilder.AddCategoriesHandler();

        //_shopImportWorkerBuilder.AddPropertyValueInterceptorsLogging(_shopCategoriesSettings, "ClassName", (shopSetting) => shopSetting.Id);

        _builder.Services.AddHttpClient();

        _builder.Services.AddScoped<ShopImportWorker>();


        var host = _builder.Build();

        var shopImportService = host.Services.GetService<ShopImportWorker>();

        Assert.NotNull(shopImportService);

        Assert.Equal(4, host.Services.GetServices<IImportService>().Count());
    }

    [Fact]
    public async Task BuildShopImportServiceWithProductsOnlyTestAsync()
    {
        await SetShopSettings();

        foreach (var shop in _shopProductsSettings)
            shop.SetAppPath(Utils.GetAppPath());

        //todo
        //_shopImportWorkerBuilder.AddShopProducts(_shopProductsSettings);

        //_shopImportWorkerBuilder.AddProductsHandler();

        _builder.Services.AddScoped<ShopImportWorker>();

        _builder.Services.AddHttpClient();

        var host = _builder.Build();

        var shopImportService = host.Services.GetService<ShopImportWorker>();

        Assert.NotNull(shopImportService);

        Assert.Equal(2, host.Services.GetServices<IImportService>().Count());
    }
}
