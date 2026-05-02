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
using Alchemist.Product.Infrastructure;
using Mediator.Infrastructure;

namespace Alchemist.Product.Import.DBService.Test;

public class ImportDBServiceTest(ITestOutputHelper outputHelper) : LoggedContextTest(outputHelper)
{
    private readonly Mock<IMediator> _mediator = new();

    //public  ImportDBServiceTest(ImportDBServiceWebAppFactory webAppFactory, ITestOutputHelper outputHelper) 
    //    : base(webAppFactory, outputHelper)
    //{
    //    WebAppFactory.FixtureLoggingContext.LoggedMessage += Log;
    //}

    private async Task<ImportDBServiceWebAppFactory> CreateWebAppFactoryAsync(string database, int[] ports)
    {
        if (ports.Length < 6)
            throw new Exception($"ports in range must be greator equals 6");
        var webAppFactory = new ImportDBServiceWebAppFactory(ports[0], ports[1],database,  ports[2], ports[3], ports[4], ports[5]);

        await webAppFactory.InitializeAsync();

        webAppFactory.FixtureLoggingContext.LoggedMessage += Log;

        return webAppFactory;
    }


    [Fact]
    public async Task HelloResponseWhenStartingSuccessAsync()
    {
        var WebAppFactory = await CreateWebAppFactoryAsync("test_import_db_hello", [8232, 8233, 8052, 8053, 8072, 8073]);

        try
        {
            await WebAppFactory.InitializeAsync();

            var httpClient = WebAppFactory.CreateClient();
            var response = await httpClient.GetAsync("/");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var hello = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello ImportDBService!", hello);
        }
        catch
        {
            await (WebAppFactory as IAsyncLifetime).DisposeAsync();
            throw;
        }
    }

    [Fact]
    public async Task WaitForImportCategoryItemReceiveByShopServiceAsync()
    {
        void setTestRepository(IServiceCollection services) => services.InterceptImplementation(_mediator.Object);

        var WebAppFactory = await CreateWebAppFactoryAsync("test_import_db_category", [8234, 8235, 8054, 8055, 8074, 8075]);

        try
        {
            WebAppFactory.ShopAPIWebAppFactory.Configure += setTestRepository;

            await WebAppFactory.InitializeAsync();

            var categoryMessage = CreateTestCategoryData();

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

            WebAppFactory.CreateClient();

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

            try
            {

                var shopCreatedWaiting = shopCategoryCreateResetEvent.WaitAsync();
                await testSender.Send(new ImportShopCategoryCommand(categoryMessage), eventName);
                await shopCreatedWaiting.WaitAsync(TimeSpan.FromMilliseconds(20000), cancellationToken: default);

                _mediator.Verify(r => r.Send(It.Is<FindByNameRequest<Data.Shop>>(r => r.Name == categoryMessage.ShopName), It.IsAny<CancellationToken>()));
                _mediator.Verify(r => r.Send(It.Is<GetShopCategoryByShopIdAndItemIdRequest>(r => r.ShopId == shop.Id && r.ItemId == categoryMessage.ShopCategory.ItemId)
                    , It.IsAny<CancellationToken>()));
            }
            catch
            {
                OutputErrors();
                OutputWarnings();

                throw;
            }
            finally
            {
                WebAppFactory.ShopAPIWebAppFactory.Configure -= setTestRepository;
            }
        }
        catch
        {
            await (WebAppFactory as IAsyncLifetime).DisposeAsync();
            throw;
        }
    }

    [Fact]
    public async Task WaitForImportProductItemReceiveByGrpcServiceAsync()
    {
        void setTestRepository(IServiceCollection services) => services.InterceptImplementation(_mediator.Object);

        var WebAppFactory = await CreateWebAppFactoryAsync("test_import_db_product", [8236, 8237, 8056, 8057, 8076, 8077]);

        WebAppFactory.GrpcWebAppFactory.ConfigureServices += setTestRepository;
        WebAppFactory.ShopAPIWebAppFactory.Configure += setTestRepository;

        var productData = CreateTestProductData();

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

        try
        {
            await WebAppFactory.InitializeAsync();

            WebAppFactory.CreateClient();

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

            try
            {
                var productFindWaiting = shopProductFindResetEvent.WaitAsync();
                await productFindWaiting.WaitAsync(TimeSpan.FromMilliseconds(20000), cancellationToken: default);

                _mediator.Verify(r => r.Send(It.Is<FindByNameRequest<Data.Shop>>(r => r.Name == productData.ShopName), It.IsAny<CancellationToken>()));
                _mediator.Verify(r => r.Send(It.Is<GetShopProductByShopAndItemIdRequest>(r => r.ShopId == shop.Id && r.ItemId == productData.ShopProduct.ItemId)
                    , It.IsAny<CancellationToken>()));
            }
            catch
            {
                OutputErrors();
                OutputWarnings();

                throw;
            }
            finally
            {
                WebAppFactory.GrpcWebAppFactory.ConfigureServices -= setTestRepository;
            }
        }
        catch
        {
            await (WebAppFactory as IAsyncLifetime).DisposeAsync();
            throw;
        }
    }

    private static AsyncAutoResetEvent SetAutoResetEventOnConnectionChanged(IMessageProcessor messageProcessor)
    {
        var autoResetEvent = new AsyncAutoResetEvent();
        messageProcessor.ConnectionStateChanged += async (sender, args) =>
        {
            autoResetEvent.Set();
        };
        return autoResetEvent;
    }

    private static BeautyAndHealthProductData CreateTestProductData()
    {
        return new BeautyAndHealthProductData()
        {

            Product = new Entities.Product
            {
                Name = $"Product_{Guid.NewGuid()}",
                Articul = Guid.NewGuid().ToString()
            },

            ShopProduct = new ShopProduct
            {
                ShopId = 1,
                ApiUrl = Guid.NewGuid().ToString(),
                ItemId = Guid.NewGuid().ToString(),
                ItemUrl = Guid.NewGuid().ToString(),
            },

            ShopName = Guid.NewGuid().ToString(),
            ShopUrl = Guid.NewGuid().ToString(),

            ProductType = new ProductType { Name = "TestProductType" }
        };
    }

    private static CategoryData.CategoryData CreateTestCategoryData()
    {
        return new CategoryData.CategoryData()
        {
            ShopCategory = new ShopCategory
            {
                Category = $"Product_{Guid.NewGuid()}",
                ShopId = 1,
                ItemId = 10,
                Url = $"{Guid.NewGuid()}"
            },
            ShopName = Guid.NewGuid().ToString(),
            ShopUrl = Guid.NewGuid().ToString()
        };
    }
}