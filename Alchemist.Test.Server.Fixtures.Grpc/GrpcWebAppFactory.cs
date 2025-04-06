using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Alchemist.Test.Server.Fixtures.Grpc;

public abstract class GrpcWebAppFactory<TEntryPoint, TDbContext> : AlchemistWebAppFactory<TEntryPoint, TDbContext>
    where TEntryPoint : class
    where TDbContext : DbContext
{
    public HttpMessageHandler HttpMessageHandler { get; private set; }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        var server = host.GetTestServer();
        HttpMessageHandler = server.CreateHandler();

        return host;
    }
}
