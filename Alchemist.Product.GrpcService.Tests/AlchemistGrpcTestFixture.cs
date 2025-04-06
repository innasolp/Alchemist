using Alchemist.Product.Data;
using Alchemist.Test.Server.Fixtures.Grpc;

namespace Alchemist.Product.GrpcService.Tests;

public class AlchemistGrpcTestFixture(AlchemistGrpcWebAppFactory webAppFactory) 
    : GrpcTestFixture<AlchemistGrpcWebAppFactory, Program, AlchemyContext>(webAppFactory)
{
    public AlchemyGrpcService.AlchemyGrpcServiceClient CreateAlchemistGrpcClient()
    {
       return new AlchemyGrpcService.AlchemyGrpcServiceClient(GrpcChannel);
    }
}
