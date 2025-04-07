using Alchemist.Product.Data;
using Alchemist.Test.Server.Fixtures.Grpc;
using Grpc.Core;
using Xunit.Abstractions;

namespace Alchemist.Product.GrpcService.Tests;

public class AlchemistGrpcServiceIntegrationTest(AlchemistGrpcWebAppFactory webAppFactory, ITestOutputHelper outputHelper)
    : GrpcTestFixture<AlchemistGrpcWebAppFactory, Program, AlchemyContext>(webAppFactory, outputHelper)
{
    [Fact]
    public async Task FindBrandByExistingNameSuccess()
    {
        var client = new AlchemyGrpcService.AlchemyGrpcServiceClient(GrpcChannel);

        var brandName = "Elizavecca";        
        var response = await client.FindBrandByNameAsync(new FindByNameRequest { Name = brandName});

        Assert.NotNull(response);
        Assert.Equal(brandName, response.Name);
    }

    [Fact]
    public async Task FindBrandByEmptyNameThrowsBadRequestRpcException()
    {
        var client = new AlchemyGrpcService.AlchemyGrpcServiceClient(GrpcChannel);

        var rpcException = await Assert.ThrowsAsync<RpcException>(async () =>
        {
            await client.FindBrandByNameAsync(new FindByNameRequest { Name = "" });
        });

        OutputHelper.WriteLine(rpcException.Message);
        OutputHelper.WriteLine(rpcException.StackTrace);

        Assert.Equal(StatusCode.InvalidArgument, rpcException.Status.StatusCode);
    }

    [Fact]
    public async Task FindBrandByNotExistingNameThrowsNotFoundRpcException()
    {
        var client = new AlchemyGrpcService.AlchemyGrpcServiceClient(GrpcChannel);

        var brandName = Guid.NewGuid().ToString();
        var rpcException = await Assert.ThrowsAsync<RpcException>(async () =>
        {
            await client.FindBrandByNameAsync(new FindByNameRequest { Name = brandName });
        });

        OutputHelper.WriteLine(rpcException.Message);
        OutputHelper.WriteLine(rpcException.StackTrace);

        Assert.Equal(StatusCode.NotFound, rpcException.Status.StatusCode);
    }

    [Fact]
    public async Task CreateBrandSuccess()
    {
        var client = new AlchemyGrpcService.AlchemyGrpcServiceClient(GrpcChannel);

        var brand = new CreateBrandRequest { Name = Guid.NewGuid().ToString() };
        var createResponse = await client.CreateBrandAsync(brand);

        Assert.NotNull(createResponse);
        Assert.Equal(brand.Name, createResponse.Name);

        var findResponse = await client.FindBrandByNameAsync(new FindByNameRequest { Name = brand.Name });
        Assert.NotNull(findResponse);
        Assert.Equal(createResponse.Id, findResponse.Id);
    }

    [Fact]
    public async Task CreateShopProductWithNonUniqueShopIdProductIdThrowsInternalRpcException()
    {
        var client = new AlchemyGrpcService.AlchemyGrpcServiceClient(GrpcChannel);

        var createShopProductRequest = new CreateShopProductRequest
        {
            Apiurl = Guid.NewGuid().ToString(),
            Itemid = Guid.NewGuid().ToString(),
            Shopid = 1,
            Productid = 1,
            Itemurl = Guid.NewGuid().ToString(),
            Price = 0.0
        };
        var reply = await client.CreateShopProductAsync(createShopProductRequest);
        
        var createShopProductRequestInvalid = new CreateShopProductRequest
        {
            Apiurl = Guid.NewGuid().ToString(),
            Itemid = Guid.NewGuid().ToString(),
            Shopid = 1,
            Productid = 1,
            Itemurl = Guid.NewGuid().ToString(),
            Price = 0.0
        };

        var rpcException = await Assert.ThrowsAsync<RpcException>(async () =>
        {
            await client.CreateShopProductAsync(createShopProductRequestInvalid);
        });

        OutputHelper.WriteLine(rpcException.Message);
        OutputHelper.WriteLine(rpcException.StackTrace);

        Assert.Equal(StatusCode.Internal, rpcException.Status.StatusCode);
    }
}
