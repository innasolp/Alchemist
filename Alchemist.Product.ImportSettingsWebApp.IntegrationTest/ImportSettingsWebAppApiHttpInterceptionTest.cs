using Alchemist.Test.ImportSettingsWebApp.Factory;
using Alchemist.Test.Log;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using Xunit.Abstractions;

namespace Alchemist.Product.ImportSettingsWebApp.IntegrationTest;

public class ImportSettingsWebAppApiLoggedFactory : ImportsettingsWebAppFullFactory, ILoggedContext
{
    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    FixtureLogContext ILoggedContext.FixtureLoggingContext => FixtureLoggingContext;

    public ImportSettingsWebAppApiLoggedFactory() : base(true, null, 7086, 7087, "SettingsWebApiLogTestDb", 8214, 8215)
    {
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {        
        base.ConfigureWebHostBuilderContext(context, services);

        FixtureLoggingContext.ConfigureServices(services);
    }
}

public class ImportSettingsWebAppApiHttpInterceptionTest(ImportSettingsWebAppApiLoggedFactory webAppFactory, ITestOutputHelper outputHelper) 
    : LoggedContextTestFixture<ImportSettingsWebAppApiLoggedFactory, ImportSettingsWebAppProgramm>(webAppFactory, outputHelper)
{
    [Fact]
    public async Task InfoMiddlewareLogSuccessAsync()
    {
        var httpClient = WebAppFactory.CreateClient();

        var url = "/Tab/1/1";
        var response = await httpClient.PostAsync(url, null);

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

        var url = "/Tab/1/0";
        var response = await httpClient.PostAsync(url, null);

        try
        {
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            Assert.Contains(LogMessages, 
                (msg) => msg.LogLevel == LogLevel.Error && 
                        msg.Exception?.Message.Contains("Invalid shopSettingType", StringComparison.InvariantCultureIgnoreCase) == true);
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }

}
