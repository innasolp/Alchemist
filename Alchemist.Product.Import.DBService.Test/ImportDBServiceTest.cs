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
using Mediator.Infrastructure.Command;
using Alchemist.Product.Infrastructure;

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
    public async Task WaitForImportCategoryItemReceiveByShopService()
    {
        void setTestRepository(IServiceCollection services) => services.InterceptImplementation(_mediator.Object);        

        WebAppFactory.ShopAPIWebAppFactory.Configure += setTestRepository;        

        var categoryMessage = new CategoryData.CategoryData();
        FillTestCategoryData(categoryMessage);

        var shop = new Data.Shop { Id = 1, Name = categoryMessage.ShopName, Url = categoryMessage.ShopUrl };

        var shopCategoryCreateResetEvent = new AsyncAutoResetEvent();
        _mediator.Setup(r => r.Send(It.Is<FindByNameRequest<Data.Shop>>(r => r.Name == categoryMessage.ShopName), It.IsAny<CancellationToken>()))
            .Returns((FindByNameRequest<Data.Shop> req, CancellationToken cancellationToken) =>
            {
                return Task.FromResult(shop.Adapt<Data.Shop>());
            });
        _mediator.Setup(r => r.Send(It.IsAny<CreateCommand<Data.ShopCategory>>(), It.IsAny<CancellationToken>()))
            .Returns((CreateCommand<Data.ShopCategory> req, CancellationToken cancellationToken) =>
            {
                shopCategoryCreateResetEvent.Set();
                return Task.FromResult(new Data.ShopCategory
                {
                    Id = 2,
                    ShopId = shop.Id,
                    ItemId = categoryMessage.ShopCategory.ItemId,
                    Category = categoryMessage.ShopCategory.Category
                });
            });
        
        var messageReceiver = WebAppFactory.Services.GetRequiredService<IMessageReceiver>();
        var connectionResetEvent = SetAutoResetEventOnConnectionChanged(messageReceiver);
        if (!messageReceiver.IsConnected)
        {
            var connectionWaiting = connectionResetEvent.WaitAsync();
            await connectionWaiting.WaitAsync(TimeSpan.FromMilliseconds(20000), cancellationToken: default);
        }

        var eventName = WebAppFactory.Configuration.GetSection("RabbitMQCategoryEvent").Get<string>();        

        await Task.Delay(5000);

        var testSender = WebAppFactory.CreateTestSender();
        await testSender.Start();        
        
        var shopCreatedWaiting = shopCategoryCreateResetEvent.WaitAsync();
        await testSender.Send(new ImportShopCategoryCommand(categoryMessage), eventName);
        await shopCreatedWaiting.WaitAsync(TimeSpan.FromMilliseconds(20000), cancellationToken: default);

        _mediator.Verify(r => r.Send(It.Is<FindByNameRequest<Data.Shop>>(r => r.Name == categoryMessage.ShopName), It.IsAny<CancellationToken>())); 
        _mediator.Verify(r => r.Send(It.Is <GetShopCategoryByShopIdAndItemIdRequest>(r=>r.ShopId == shop.Id && r.ItemId == categoryMessage.ShopCategory.ItemId)
            ,It.IsAny<CancellationToken>()));
       
        WebAppFactory.ShopAPIWebAppFactory.Configure += setTestRepository;
    }

    [Fact]
    public async Task WaitForImportProductItemReceiveByGrpcService()
    {
        void setTestRepository(IServiceCollection services) => services.InterceptImplementation(_mediator.Object);        

        WebAppFactory.GrpcWebAppFactory.ConfigureServices += setTestRepository;
        WebAppFactory.ShopAPIWebAppFactory.Configure += setTestRepository;

        var productData = new BeautyAndHealthProductData();
        FillTestProductData(productData);

        var shop = new Data.Shop { Id = 1, Name = productData.ShopName, Url = productData.ShopUrl };

        var shopProductFindResetEvent = new AsyncAutoResetEvent();
        _mediator.Setup(r => r.Send(It.Is<FindByNameRequest<Data.Shop>>(r => r.Name == productData.ShopName), It.IsAny<CancellationToken>()))
            .Returns((FindByNameRequest<Data.Shop> req, CancellationToken cancellationToken) =>
            {
                return Task.FromResult(shop.Adapt<Data.Shop>());
            });
        _mediator.Setup(r => r.Send(It.IsAny<GetShopProductByShopAndItemIdRequest>(), It.IsAny<CancellationToken>()))
            .Returns((GetShopProductByShopAndItemIdRequest req, CancellationToken cancellationToken) =>
            {
                shopProductFindResetEvent.Set();
                return Task.FromResult(productData.ShopProduct.Adapt<Data.ShopProduct>());
            });

        WebAppFactory.StartGrpc();

        var messageReceiver = WebAppFactory.Services.GetRequiredService<IMessageReceiver>();
        var connectionResetEvent = SetAutoResetEventOnConnectionChanged(messageReceiver);
        if (!messageReceiver.IsConnected)
        {
            var connectionWaiting = connectionResetEvent.WaitAsync();
            await connectionWaiting.WaitAsync(TimeSpan.FromMilliseconds(20000), cancellationToken: default);
        }

        var eventName = WebAppFactory.Configuration.GetSection("RabbitMQProductEvent").Get<string>();

        await Task.Delay(2000);

        var testSender = WebAppFactory.CreateTestSender();
        await testSender.Start();

        await testSender.Send(new ImportBeautyAndHealthProductCommand(productData), eventName);

        var productFindWaiting = shopProductFindResetEvent.WaitAsync();
        await productFindWaiting.WaitAsync(TimeSpan.FromMilliseconds(20000), cancellationToken: default);

        _mediator.Verify(r => r.Send(It.Is<FindByNameRequest<Data.Shop>>(r => r.Name == productData.ShopName), It.IsAny<CancellationToken>())); 
        _mediator.Verify(r => r.Send(It.Is<GetShopProductByShopAndItemIdRequest>(r => r.ShopId == shop.Id && r.ItemId == productData.ShopProduct.ItemId)
            , It.IsAny<CancellationToken>()));

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

    private static void FillTestProductData(BeautyAndHealthProductData productData)
    {
        productData.Product = new Entities.Product {
            Name = $"Product_{Guid.NewGuid()}",
            Articul = Guid.NewGuid().ToString()
        };

        productData.ShopProduct = new ShopProduct
        {
            ShopId = 1,
            ApiUrl = Guid.NewGuid().ToString(),
            ItemId = Guid.NewGuid().ToString(),
            ItemUrl = Guid.NewGuid().ToString(),
        };

        productData.ShopName = Guid.NewGuid().ToString();
        productData.ShopUrl = Guid.NewGuid().ToString();
    }

    private static void FillTestCategoryData(CategoryData.CategoryData categoryData)
    {
        categoryData.ShopCategory = new ShopCategory {
            Category = $"Product_{Guid.NewGuid()}",
            ShopId =1,
            ItemId = 10,
            Url = $"{Guid.NewGuid()}"
        };
        
        categoryData.ShopName = Guid.NewGuid().ToString();
        categoryData.ShopUrl = Guid.NewGuid().ToString();
    }
}