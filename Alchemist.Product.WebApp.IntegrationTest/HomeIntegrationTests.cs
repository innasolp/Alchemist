using Alchemist.Test.Log;
using Alchemist.Test.ProductWebAppFactory;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Test.PostresqlTestContainer;
using Xunit.Abstractions;
using TestCommon = Alchemist.Product.WebApp.IntegrationTest.Infrastructure.Common;

namespace Alchemist.Product.WebApp.IntegrationTest;

public class TestProductWebAppFactory : ProductAggregatorConfigurationWebAppFactory<PostgresqlTestDbContainer, PostgresDbRespawner, PostgresDbChecker>, ILoggedContext
{
    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    FixtureLogContext ILoggedContext.FixtureLoggingContext => FixtureLoggingContext;

    public TestProductWebAppFactory()
        : base(7102, 7103,
            8060, 8061,
            7500, 7501,
            "ConnectionStrings:DbContext2", Common.ConfigurationHelper.GetSectionValue("ContainerWebAppTestDb"),
            8202, 8203,
            8406, 8407,
            "ConnectionStrings:DbContext2", Common.ConfigurationHelper.GetSectionValue("ContainerWebAppTestDb"),
            TestCommon.SignalRTestServer)
    {
    }
    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        FixtureLoggingContext.ConfigureServices(services);
    }
}

public class HomeIntegrationTests(TestProductWebAppFactory factory, ITestOutputHelper outputHelper)
    : LoggedContextTestFixture<TestProductWebAppFactory, ProductWebAppProgramm>(factory, outputHelper)
{
    [Fact]
    public async Task Get_Root_Redirects_To_ShopRoot()
    {
        try
        {
            var client = WebAppFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            var resp = await client.GetAsync("/");

            Assert.Equal(HttpStatusCode.Redirect, resp.StatusCode);
            Assert.NotNull(resp.Headers.Location);
            Assert.Equal("/Shop/", resp.Headers.Location!.ToString());
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }

    [Fact]
    public async Task Get_ShopRoot_Returns_OK()
    {
        try
        {
            var client = WebAppFactory.CreateClient();

            var resp = await client.GetAsync("/Shop/");

            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var content = await resp.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }

    [Fact]
    public async Task Get_ImportSettingsRoot_Returns_OK()
    {
        try
        {
            var client = WebAppFactory.CreateClient();

            var resp = await client.GetAsync("/Import/Settings");

            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            var content = await resp.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }
}