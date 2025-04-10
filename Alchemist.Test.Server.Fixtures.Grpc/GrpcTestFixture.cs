using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace Alchemist.Test.Server.Fixtures.Grpc;

public abstract class GrpcTestFixture<TGrpcWebAppFactory, TEntryPoint> :
    TestFixture<TGrpcWebAppFactory, TEntryPoint>
    where TEntryPoint : class   
    where TGrpcWebAppFactory : WebApplicationFactory<TEntryPoint>
{
    protected GrpcChannel GrpcChannel { get; private set; }

    protected HttpMessageHandler HttpMessageHandler { get; private set; }

    public GrpcTestFixture(TGrpcWebAppFactory webAppFactory, ITestOutputHelper outputHelper) : base(webAppFactory, outputHelper)
    {
        HttpMessageHandler = WebAppFactory.Server.CreateHandler();
        
        GrpcChannel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
        {
             HttpHandler = HttpMessageHandler
         });
    }

    public override void Dispose()
    {
        GrpcChannel.Dispose();
        HttpMessageHandler.Dispose();
        base.Dispose();
    }
}
