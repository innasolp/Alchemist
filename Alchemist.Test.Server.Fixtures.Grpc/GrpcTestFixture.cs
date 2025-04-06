using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;

namespace Alchemist.Test.Server.Fixtures.Grpc;

public abstract class GrpcTestFixture<TGrpcWebAppFactory, TEntryPoint, TDbContext> :
    TestFixture<TGrpcWebAppFactory, TEntryPoint, TDbContext>
    where TEntryPoint : class
    where TDbContext : DbContext
    where TGrpcWebAppFactory : AlchemistWebAppFactory<TEntryPoint, TDbContext>
{
    protected GrpcChannel GrpcChannel { get; private set; }

    protected HttpMessageHandler HttpMessageHandler { get; private set; }

    public GrpcTestFixture(TGrpcWebAppFactory webAppFactory) : base(webAppFactory)
    {
        HttpMessageHandler = WebAppFactory.Server.CreateHandler();
        
        GrpcChannel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
        {
             HttpHandler = HttpMessageHandler
         });
    }
}
