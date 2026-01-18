using Alchemist.Product.BeautyAndHealth;
using Alchemist.Product.CategoryData;
using Alchemist.Product.Entities;
using Alchemist.Test.Server.Fixtures;
using Mediator.Infrastructure.Request;
using MediatR;
using Message.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Threading;
using Moq;
using System.Net;
using Xunit.Abstractions;
using Mapster;
using Shop.Infrastructure;

namespace Alchemist.Product.Import.DBService.Test;

public class ImportDBServiceTest(ImportDBServiceWebAppFactory webAppFactory, ITestOutputHelper outputHelper) 
    : TestFixture<ImportDBServiceWebAppFactory, ImportDbServiceProgram>(webAppFactory, outputHelper)
{

    private readonly Mock<IMediator> _mediator = new();

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
        void setTestRepository(IServiceCollection services) => services.InterceptImplementation(_mediator.Object);
        WebAppFactory.GrpcWebAppFactory.ConfigureServices += setTestRepository;
        WebAppFactory.ShopAPIWebAppFactory.Configure += setTestRepository;        

        var categoryMessageMock = new Mock<ICategoryData>();
        FillTestCategoryData(categoryMessageMock);

        var shop = new Data.Shop { Id = 1, Name = categoryMessageMock.Object.ShopName, Url = categoryMessageMock.Object.ShopUrl };

        var shopCreateResetEvent = new AsyncAutoResetEvent();
        _mediator.Setup(r => r.Send(It.Is<FindByNameRequest<Data.Shop>>(r=>r.Name == categoryMessageMock.Object.ShopName), It.IsAny<CancellationToken>()))
            .Returns((FindByNameRequest<Data.Shop> req, CancellationToken cancellationToken) =>
            {
                shopCreateResetEvent.Set();
                return Task.FromResult(shop.Adapt<Data.Shop>());
            });

        WebAppFactory.StartGrpc();
        
        var messageReceiver = WebAppFactory.Services.GetRequiredService<IMessageReceiver>();
        var connectionResetEvent = SetAutoResetEventOnConnectionChanged(messageReceiver);
        if (!messageReceiver.IsConnected)        
            await connectionResetEvent.WaitAsync();

        await Task.Delay(1000);

        var eventName = WebAppFactory.Configuration.GetSection("RabbitMQCategoryEvent").Get<string>(); 
        var testSender = WebAppFactory.CreateTestSender();
        await testSender.Start();
        await testSender.Send(categoryMessageMock.Object, eventName);

        await shopCreateResetEvent.WaitAsync();

        _mediator.Verify(r => r.Send(It.Is<FindByNameRequest<Data.Shop>>(r => r.Name == categoryMessageMock.Object.ShopName), It.IsAny<CancellationToken>()));

        await Task.Delay(500);

        _mediator.Verify(r => r.Send(It.Is <GetShopCategoryByShopIdAndItemIdRequest>(r=>r.ShopId == shop.Id && r.ItemId == categoryMessageMock.Object.ShopCategory.ItemId)
            ,It.IsAny<CancellationToken>()));

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

    private static void FillTestProductData(Mock<IBeautyAndHealthProductData> productDataMock)
    {
        productDataMock.Setup(p => p.Product).Returns(new Entities.Product {
            Name = $"Product_{Guid.NewGuid()}",
            Articul = Guid.NewGuid().ToString()
        });
        productDataMock.Setup(p => p.ShopProduct).Returns(new ShopProduct
        {
            ShopId = 1,
            ApiUrl = Guid.NewGuid().ToString(),
            ItemId = Guid.NewGuid().ToString(),
            ItemUrl = Guid.NewGuid().ToString(),
        });
        productDataMock.Setup(p => p.ShopName).Returns(Guid.NewGuid().ToString());
        productDataMock.Setup(p => p.ShopUrl).Returns(Guid.NewGuid().ToString());
    }

    private static void FillTestCategoryData(Mock<ICategoryData> categoryDataMock)
    {
        categoryDataMock.Setup(p => p.ShopCategory).Returns(new ShopCategory {
            Category = $"Product_{Guid.NewGuid()}",
            ShopId =1,
            ItemId = 10
        });
        
        categoryDataMock.Setup(p => p.ShopName).Returns(Guid.NewGuid().ToString());
        categoryDataMock.Setup(p => p.ShopUrl).Returns(Guid.NewGuid().ToString());
    }
}