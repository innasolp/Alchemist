using Alchemist.DataService.Interfaces;
using Alchemist.Product.GrpcServiceClient;
using Alchemist.Test.Server.Fixtures.Grpc;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Alchemist.Product.GrpcService.Tests;

public class AlchemistGrpcTestFixture(AlchemistGrpcWebAppFactory webAppFactory) 
    : GrpcTestFixture<AlchemistGrpcWebAppFactory, Program>(webAppFactory)
{
    public IProductDataService CreateAlchemistGrpcClient()
    {
        return new AlchemyGrpcServiceClient(GrpcChannel);
    }
}
