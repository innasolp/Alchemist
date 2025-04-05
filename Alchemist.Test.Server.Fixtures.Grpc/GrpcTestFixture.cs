using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Alchemist.Test.Server.Fixtures.Grpc;

public abstract class GrpcTestFixture<TWebAppFactory, TProgram> : IClassFixture<TWebAppFactory>
    where TProgram: Program
    where TWebAppFactory: WebApplicationFactory<TProgram>
{
    protected TWebAppFactory WebAppFactory { get; private set; }

    protected GrpcChannel GrpcChannel { get; private set; }

    public GrpcTestFixture(TWebAppFactory webAppFactory)
    {
        WebAppFactory = webAppFactory;

        var loggerFactory = webAppFactory.Services.GetRequiredService<ILoggerFactory>();

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (req, cert, chain, errors) =>
            {
                return true;
            }
        };

        GrpcChannel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
        {
            LoggerFactory = loggerFactory,
            HttpHandler = handler
        });
    }
}
