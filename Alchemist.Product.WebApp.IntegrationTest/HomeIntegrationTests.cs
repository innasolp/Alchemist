using Alchemist.Product.WebApp.IntegrationTest.Infrastructure;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit.Abstractions;

namespace Alchemist.Product.WebApp.IntegrationTest;

public class TestProductWebAppFactory : ProductWebAppTestContainerLifetimeFactory
{
    public TestProductWebAppFactory() : base(7102, 7103,
        8060, 8061,
        7500, 7501,
      8406, 8407,
      7088, 7089,
        Common.ConfigurationHelper.GetSectionValue("ContainerWebAppTestDb"))
    { }
}

public class HomeIntegrationTests(TestProductWebAppFactory factory, ITestOutputHelper outputHelper) : TestFixture<TestProductWebAppFactory, ProductWebAppProgramm>(factory, outputHelper)
{
    [Fact]
    public async Task Get_Root_Redirects_To_ShopRoot()
    {
        // prevent automatic redirect so we can assert location header
        var client = WebAppFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var resp = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, resp.StatusCode);
        Assert.NotNull(resp.Headers.Location);
        Assert.Equal("/Shop/", resp.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Get_ShopRoot_Returns_OK()
    {
        var client = WebAppFactory.CreateClient();

        var resp = await client.GetAsync("/Shop/");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        // basic sanity: page content should be non-empty
        var content = await resp.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(content));
    }

    [Fact]
    public async Task Get_ImportSettingsRoot_Returns_OK()
    {
        var client = WebAppFactory.CreateClient();

        var resp = await client.GetAsync("/Import/Settings");

        // controller action for Import/Settings should return view result; at minimum expect 200 OK.
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var content = await resp.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(content));
    }
}