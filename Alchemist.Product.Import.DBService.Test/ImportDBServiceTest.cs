using Alchemist.DataService.Interfaces;
using Alchemist.Product.ImportItem.Interfaces;
using Alchemist.Test.Server.Fixtures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        WebAppFactory.StartHttpClients();
        var httpClient = WebAppFactory.CreateClient();

        var response = await httpClient.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var hello = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello ImportDBService!", hello);
    }

    [Fact]
    public async Task WaitForImportItemReceiveByGrpcService()
    {
        WebAppFactory.GrpcWebAppFactory.ConfigureServices += SetTestRepository;

        WebAppFactory.StartHttpClients();

        var productMessageMock = new Mock<IProductData>();
        FillTestProductData(productMessageMock);

        var testSender = WebAppFactory.CreateTestSender();
        await testSender.Start();
        testSender.Send(productMessageMock.Object, WebAppFactory.Configuration.GetSection("RabbitMQProductEvent").Get<string>());

        await Task.Delay(5000);

        _alchemyRepositoryMock.Verify(r => r.GetShopProductByShopAndItemId(productMessageMock.Object.ShopId, productMessageMock.Object.ShopProduct.ItemId));

        WebAppFactory.GrpcWebAppFactory.ConfigureServices -= SetTestRepository;
    }

    private void SetTestRepository(IServiceCollection services)    
    {
        var sd = services.FirstOrDefault(s => s.ServiceType == typeof(IAlchemyRepository));
        if(sd != null)
            services.Remove(sd);

        services.AddSingleton<IAlchemyRepository>(_alchemyRepositoryMock.Object);
    }

    private static void FillTestProductData(Mock<IProductData> productDataMock)
    {
        productDataMock.Setup(p => p.ShopId).Returns(1);
        productDataMock.Setup(p => p.Product).Returns(new Entities.Product {
            Name = $"Product_{Guid.NewGuid().ToString()}",
            Articul = Guid.NewGuid().ToString()
        });
        productDataMock.Setup(p => p.ShopProduct).Returns(new Entities.ShopProduct
        {
            ShopId = 1,
            ApiUrl = Guid.NewGuid().ToString(),
            ItemId = Guid.NewGuid().ToString(),
            ItemUrl = Guid.NewGuid().ToString(),
        });
    }
}