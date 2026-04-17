using Alchemist.Test.Log;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Shop.API.Test.Infrastructure;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Shop.API.Test;

public class ShopAPISignlRMockWebAppFactory : ShopApiConfigurationWebAppFactory, ILoggedContext
{
    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    FixtureLogContext ILoggedContext.FixtureLoggingContext => FixtureLoggingContext;

    public ShopAPISignlRMockWebAppFactory() 
        : base("ConnectionStrings:DbContext2", "test_shop", SignalRCommon.ConfigureSignalRMock)
    { 
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        FixtureLoggingContext.ConfigureServices(services);
    }
}

public class ShopAPIIntegrationTest(ShopAPISignlRMockWebAppFactory webAppFactory, ITestOutputHelper outputHelper) 
    : LoggedContextTestFixture<ShopAPISignlRMockWebAppFactory, ShopAPIProgram>(webAppFactory, outputHelper)
{
    [Fact]
    public async Task GetShopSuccessAsync()
    {
        var name = "TestShop";

        try
        {
            using var httpClient = WebAppFactory.GetHostHttpClient();
            var response = await httpClient.GetAsync($"api/Shop/byName?name={name}");
            response.EnsureSuccessStatusCode();
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            var shop = await response.Content.ReadFromJsonAsync<Alchemist.Product.Data.Shop>();
            Assert.NotNull(shop);
            Assert.Equal(name, shop.Name);
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
        finally
        {
           //await WebAppFactory.ResetDatabaseIfAvailableAsync();
        }
    }

    [Fact]
    public async Task ResponseStatusBadRequestOnGetShopWithEmptyNameAsync()
    {
        try
        {
            using var httpClient = WebAppFactory.GetHostHttpClient();
            var response = await httpClient.GetAsync($"api/Shop/byName?name={""}");
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
        finally
        {
            //await WebAppFactory.ResetDatabaseIfAvailableAsync();
        }
    }

    [Fact]
    public async Task ResponseStatusNotFoundOnGetShopWithNotExistsNameAsync()
    {
        try
        {
            using var httpClient = WebAppFactory.GetHostHttpClient();
            var response = await httpClient.GetAsync($"api/Shop/byName?name={Guid.NewGuid().ToString()}");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
        finally
        {
            //await WebAppFactory.ResetDatabaseIfAvailableAsync();
        }
    }

    [Fact]
    public async Task CreateShopSuccessAsync()
    {
        var shop = new Alchemist.Product.Data.Shop() { Name = "TestShopNew", Url = "https://testshopnew" };
            
        try
        {
            using var httpClient = WebAppFactory.GetHostHttpClient();
            var response = await httpClient.PutAsJsonAsync($"api/Shop", shop);

            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

            var getNewResponse = await httpClient.GetAsync($"api/Shop/byName?name={shop.Name}");
            getNewResponse.EnsureSuccessStatusCode();
            var shopNew = await getNewResponse.Content.ReadFromJsonAsync<Alchemist.Product.Data.Shop>();
            Assert.NotNull(shopNew);
            Assert.Equal(shop.Name, shopNew.Name);
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
        finally
        {
            //await WebAppFactory.ResetDatabaseIfAvailableAsync();
        }
    }

    [Fact]
    public async Task ResponseInternalErrorOnCreateShopWithExistsIdAsync()
    {
        var shop = new Alchemist.Product.Data.Shop() { Name = "TestShopNew", Url = "https://testshopnew", Id = 1 };

        try
        {
            using var httpClient = WebAppFactory.GetHostHttpClient();
            var response = await httpClient.PutAsJsonAsync($"api/Shop", shop);

            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
        finally
        {
            //await WebAppFactory.ResetDatabaseIfAvailableAsync();
        }
    }
}