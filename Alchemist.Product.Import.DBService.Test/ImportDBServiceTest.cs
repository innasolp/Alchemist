using Alchemist.Product.BeautyAndHealth;
using Alchemist.Product.CategoryData;
using Alchemist.Product.Entities;
using Alchemist.Test.Server.Fixtures;
using Message.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Threading;
using Moq;
using System.Net;
using Xunit.Abstractions;
using Mapster;
using Shop.Data.Infrastructure;
using Db.Infrastructure.Commands;
using Db.Infrastructure.Requests;
using Product.Data.Infrastructure;
using Db.Infrastructure;
using Autofac.Core;
using Autofac;

namespace Alchemist.Product.Import.DBService.Test;

public class ImportDBServiceTest(ITestOutputHelper outputHelper) : LoggedContextTest(outputHelper)
{
    private async Task<ImportDBServiceWebAppFactory> CreateWebAppFactoryAsync(string database, int[] ports)
    {
        if (ports.Length < 6)
            throw new Exception($"ports in range must be greator equals 6");
        var webAppFactory = new ImportDBServiceWebAppFactory(ports[0], ports[1],database,  ports[2], ports[3], ports[4], ports[5]);

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
        var findShopByNameRequestHandlerMock = new Mock<IRequestHandler<FindByNameRequest<Data.Shop>, Data.Shop?>>();
        var createShopCategoryCommandMock = new Mock<ICommandHandler<CreateCommand<Data.ShopCategory>>>();
        var getShopCategoryByShopIdAndItemIdMock = new Mock<IRequestHandler<GetShopCategoryByShopIdAndItemIdRequest, Data.ShopCategory>>();
        void configureShopApiContainer(ContainerBuilder containerBuilder)
            {
                containerBuilder
                    .RegisterInstance(findShopByNameRequestHandlerMock.Object)
                    .As<IRequestHandler<FindByNameRequest<Data.Shop>, Data.Shop>>();
                containerBuilder
                    .RegisterInstance(createShopCategoryCommandMock.Object)
                    .As<ICommandHandler<CreateCommand<Data.ShopCategory>>>();
                containerBuilder
                    .RegisterInstance(getShopCategoryByShopIdAndItemIdMock.Object)
                    .As<IRequestHandler<GetShopCategoryByShopIdAndItemIdRequest, Data.ShopCategory>>();
            }            

        var WebAppFactory = await CreateWebAppFactoryAsync("test_import_db_category", [8234, 8235, 8054, 8055, 8074, 8075]);

        try
        {
            var categoryMessage = CreateTestCategoryData();

            var shop = new Data.Shop { Id = 1, Name = categoryMessage.ShopName, Url = categoryMessage.ShopUrl };
            
            WebAppFactory.ShopAPIWebAppFactory.ConfigureContainer += configureShopApiContainer;            

            findShopByNameRequestHandlerMock.Setup(r => r.Handle(It.Is<FindByNameRequest<Data.Shop>>(r => r.Name == categoryMessage.ShopName), 
                It.IsAny<CancellationToken>()))
                .Returns((FindByNameRequest<Data.Shop> req, CancellationToken cancellationToken) =>
                {
                    return Task.FromResult(shop.Adapt<Data.Shop>());
                });

            var shopCategoryCreateResetEvent = new AsyncAutoResetEvent();
            createShopCategoryCommandMock.Setup(r => r.Handle(It.IsAny<CreateCommand<Data.ShopCategory>>(), It.IsAny<CancellationToken>()))
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

            await WebAppFactory.InitializeAsync();

            WebAppFactory.CreateClient();

            var messageReceiver = WebAppFactory.Services.GetRequiredService<IMessageReceiver>();
            var connectionResetEvent = SetAutoResetEventOnConnectionChanged(messageReceiver);
            if (!messageReceiver.IsConnected)
            {
                var connectionWaiting = connectionResetEvent.WaitAsync();
                await connectionWaiting.WaitAsync(TimeSpan.FromMilliseconds(40000), cancellationToken: default);
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

                findShopByNameRequestHandlerMock.Verify(r => r.Handle(It.Is<FindByNameRequest<Data.Shop>>(r => r.Name == categoryMessage.ShopName), It.IsAny<CancellationToken>()));
                getShopCategoryByShopIdAndItemIdMock.Verify(r => r.Handle(It.Is<GetShopCategoryByShopIdAndItemIdRequest>(r => r.ShopId == shop.Id && r.ItemId == categoryMessage.ShopCategory.ItemId)
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
                WebAppFactory.ShopAPIWebAppFactory.ConfigureContainer -= configureShopApiContainer;
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
        var findShopByNameRequestHandlerMock = new Mock<IRequestHandler<FindByNameRequest<Data.Shop>, Data.Shop>>();
        var createShopCategoryCommandMock = new Mock<ICommandHandler<CreateCommand<Data.ShopCategory>>>();
        var getShopProductByShopIdAndItemIdMock = new Mock<IRequestHandler<GetShopProductByShopAndItemIdRequest, Data.ShopProduct>>();
        var WebAppFactory = await CreateWebAppFactoryAsync("test_import_db_product", [8236, 8237, 8056, 8057, 8076, 8077]);

        void configureGrpcContainer(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterInstance(getShopProductByShopIdAndItemIdMock.Object)
                .As<IRequestHandler<GetShopProductByShopAndItemIdRequest, Data.ShopProduct>>();
        }
        WebAppFactory.GrpcWebAppFactory.ConfigureContainer += configureGrpcContainer;

        void configureShopApiContainer(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterInstance(findShopByNameRequestHandlerMock.Object)
                .As<IRequestHandler<FindByNameRequest<Data.Shop>, Data.Shop>>();
            containerBuilder
                .RegisterInstance(createShopCategoryCommandMock.Object)
                .As<ICommandHandler<CreateCommand<Data.ShopCategory>>>();
        }
        WebAppFactory.ShopAPIWebAppFactory.ConfigureContainer += configureShopApiContainer;

        var productData = CreateTestProductData();

        var shop = new Data.Shop { Id = 1, Name = productData.ShopName, Url = productData.ShopUrl };

        var shopProductFindResetEvent = new AsyncAutoResetEvent();
        findShopByNameRequestHandlerMock.Setup(r => r.Handle(It.Is<FindByNameRequest<Data.Shop>>(r => r.Name == productData.ShopName), It.IsAny<CancellationToken>()))
            .Returns((FindByNameRequest<Data.Shop> req, CancellationToken cancellationToken) =>
            {
                return Task.FromResult(shop.Adapt<Data.Shop>());
            });
        getShopProductByShopIdAndItemIdMock.Setup(r => r.Handle(It.IsAny<GetShopProductByShopAndItemIdRequest>(), It.IsAny<CancellationToken>()))
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
                await connectionWaiting.WaitAsync(TimeSpan.FromMilliseconds(30000), cancellationToken: default);
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

                findShopByNameRequestHandlerMock.Verify(r => r.Handle(It.Is<FindByNameRequest<Data.Shop>>(r => r.Name == productData.ShopName), It.IsAny<CancellationToken>()));
                getShopProductByShopIdAndItemIdMock.Verify(r => r.Handle(It.Is<GetShopProductByShopAndItemIdRequest>(r => r.ShopId == shop.Id && r.ItemId == productData.ShopProduct.ItemId)
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
                WebAppFactory.GrpcWebAppFactory.ConfigureContainer -= configureGrpcContainer;
                WebAppFactory.ShopAPIWebAppFactory.ConfigureContainer -= configureShopApiContainer;
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