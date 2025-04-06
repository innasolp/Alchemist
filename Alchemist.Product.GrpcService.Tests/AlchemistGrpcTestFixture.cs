using Alchemist.DataService.Interfaces;
using Alchemist.Product.Data;
using Alchemist.Product.GrpcServiceClient;
using Alchemist.Test.Server.Fixtures.Grpc;

namespace Alchemist.Product.GrpcService.Tests;

public class AlchemistGrpcTestFixture(AlchemistGrpcWebAppFactory webAppFactory) 
    : GrpcTestFixture<AlchemistGrpcWebAppFactory, Program, AlchemyContext>(webAppFactory)
{
    public IProductDataService CreateAlchemistGrpcClient()
    {
       return new AlchemyGrpcServiceClient(GrpcChannel);
    }
}
