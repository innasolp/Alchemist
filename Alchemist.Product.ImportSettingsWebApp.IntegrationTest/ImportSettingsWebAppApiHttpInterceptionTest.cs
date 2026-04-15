using Alchemist.Product.ImportSettingsWebApp.IntegrationTest.Infrastructure;
using Alchemist.Test.Server.Fixtures;
using Microsoft.Extensions.Logging;
using System.Net;
using Xunit.Abstractions;

namespace Alchemist.Product.ImportSettingsWebApp.IntegrationTest;

public class ImportSettingsWebAppApiLoggedFactory : ImportSettingsWebAppLoggedFactory
{
    public ImportSettingsWebAppApiLoggedFactory()
        : base(true,
            7086, 7087,
            8214, 8215,
            settingsDbConnectionStringSection: Alchemist.Common.ConfigurationHelper.GetSectionValue("SettingsWebApiLogTestDb"),
            shopDbConnectionStringSection : Alchemist.Common.ConfigurationHelper.GetSectionValue("ShopWebApiLogTestDb"))
    {
    }
}

public class ImportSettingsWebAppApiHttpInterceptionTest(ImportSettingsWebAppApiLoggedFactory webAppFactory, ITestOutputHelper outputHelper)
    : LoggedContextTestFixture<ImportSettingsWebAppApiLoggedFactory, ImportSettingsWebAppProgramm>(webAppFactory, outputHelper)
{
    [Fact]
    public async Task InfoMiddlewareLogSuccessAsync()
    {
        var url = "/ImportSettingsApi/Import/Settings/1/1";

        try
        {
            var httpClient = WebAppFactory.CreateClient();
            var response = await httpClient.PostAsync(url, null);

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
}