using Alchemist.Product.ShopWebApp.Controllers;
using Alchemist.Product.ShopWebApp.IntegratonTest.Infrastructure;
using Alchemist.Test.Server.Fixtures;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Alchemist.Product.ShopWebApp.IntegratonTest;

public class ShopWebApiHttpInterceptionTest(ShopWebAppApiLoggedFactory webAppFactory, ITestOutputHelper outputHelper) 
    : LoggedContextTestFixture<ShopWebAppApiLoggedFactory, ShopWebAppProgram>(webAppFactory, outputHelper)
{
    [Fact]
    public async Task InfoMiddlewareLogSuccessAsync()
    {
        var httpClient = WebAppFactory.CreateClient();

        var data = new ShopApiData { HRefFormat = "/Shop/{0}" };
        var url = $"/ShopApi/ShopList";
        var response = await httpClient.PostAsync(url, JsonContent.Create(data));

        try
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.Contains(LogMessages, (msg) => msg.LogLevel == LogLevel.Information && msg.Message.Contains(url));
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }

    [Fact]
    public async Task GlobalExceptionHandlerLogSuccessAsync()
    {
        var httpClient = WebAppFactory.CreateClient();

        var data = new ShopApiData { HRefFormat = "/Shop/{0}" , ShopId = new Random().Next(100, 1000) };
        var url = $"/ShopApi/ShopList";
        var response = await httpClient.PostAsync(url, JsonContent.Create(data));

        try
        {
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            Assert.Contains(LogMessages, 
                (msg) => msg.LogLevel == LogLevel.Error && msg.Exception?.Message.Contains("not found") == true);
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }

}
