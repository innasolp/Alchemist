using Alchemist.DataService.Interfaces;
using Alchemist.Product.Data.Repository;
using Alchemist.Product.DataItem.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Alchemist.Test.Server.Fixtures;
using Message.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Threading;
using Moq;
using System.Net;
using Xunit.Abstractions;

namespace Alchemist.Product.Import.DBService.Test;

public class ImportDBServiceTest(ImportDBServiceWebAppFactory webAppFactory, ITestOutputHelper outputHelper) 
    : TestFixture<ImportDBServiceWebAppFactory, ImportDbServiceProgram>(webAppFactory, outputHelper)
{

    private readonly Mock<IAlchemyRepository> _alchemyRepositoryMock = new();

    [Fact]
    public async Task HelloResponseWhenStartingSuccess()
    {
        WebAppFactory.StartGrpc();
        var httpClient = WebAppFactory.CreateClient();

        var response = await httpClient.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var hello = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello ImportDBService!", hello);
    }

    [Fact]
    public async Task WaitForImportItemReceiveByGrpcService()
    {
        void setTestRepository(IServiceCollection services) => services.InterceptImplementation<IAlchemyRepository, AlchemyRepository>(_alchemyRepositoryMock.Object);
        WebAppFactory.GrpcWebAppFactory.ConfigureServices += setTestRepository;
        WebAppFactory.ShopAPIWebAppFactory.Configure += setTestRepository;        

        var productMessageMock = new Mock<IProductData>();
        FillTestProductData(productMessageMock);

        var shop = new Shop { Id = 1, Name = productMessageMock.Object.ShopName, Url = productMessageMock.Object.ShopUrl };

        var shopCreateResetEvent = new AsyncAutoResetEvent();
        _alchemyRepositoryMock.Setup(r => r.GetShopByName(productMessageMock.Object.ShopName))
            .Returns(async (string name) =>
            {
                shopCreateResetEvent.Set();
                return await Task.FromResult(shop as IShop);
            });

        WebAppFactory.StartGrpc();
        
        var messageReceiver = WebAppFactory.Services.GetRequiredService<IMessageReceiver>();
        var connectionResetEvent = SetAutoResetEventOnConnectionChanged(messageReceiver);
        if (!messageReceiver.IsConnected)        
            await connectionResetEvent.WaitAsync();

        await Task.Delay(1000);

        var eventName = WebAppFactory.Configuration.GetSection("RabbitMQProductEvent").Get<string>(); 
        var testSender = WebAppFactory.CreateTestSender();
        await testSender.Start();
        await testSender.Send(productMessageMock.Object, eventName);

        await shopCreateResetEvent.WaitAsync();

        _alchemyRepositoryMock.Verify(r => r.GetShopByName(productMessageMock.Object.ShopName));

        await Task.Delay(500);

        _alchemyRepositoryMock.Verify(r => r.GetShopProductByShopAndItemId(shop.Id, productMessageMock.Object.ShopProduct.ItemId));

        WebAppFactory.GrpcWebAppFactory.ConfigureServices -= setTestRepository;
    }

    private static AsyncAutoResetEvent SetAutoResetEventOnConnectionChanged(IMessageProcessor messageProcessor)
    {
        var autoResetEvent = new AsyncAutoResetEvent();
        messageProcessor.ConnectionChanged += async (sender, args) =>
        {
            autoResetEvent.Set();
        };
        return autoResetEvent;
    }

    private static void FillTestProductData(Mock<IProductData> productDataMock)
    {
        productDataMock.Setup(p => p.Product).Returns(new Entities.Product {
            Name = $"Product_{Guid.NewGuid()}",
            Articul = Guid.NewGuid().ToString()
        });
        productDataMock.Setup(p => p.ShopProduct).Returns(new Entities.ShopProduct
        {
            ShopId = 1,
            ApiUrl = Guid.NewGuid().ToString(),
            ItemId = Guid.NewGuid().ToString(),
            ItemUrl = Guid.NewGuid().ToString(),
        });
        productDataMock.Setup(p => p.ShopName).Returns(Guid.NewGuid().ToString());
        productDataMock.Setup(p => p.ShopUrl).Returns(Guid.NewGuid().ToString());
    }
}