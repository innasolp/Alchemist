using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;

namespace Alchemist.Product.Import.WebApp.IntegrationTest;

public class TestImportWebAppFactory() : TestWebAppFactory<ImportWebAppProgram>
{  
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices((context, services) =>
        {
            context.SetKestrelLocalhostPortsConfig(8110, 8111);
        });
    }
}
