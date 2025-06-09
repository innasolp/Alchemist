using Alchemist.Product.GrpcService.Tests.Infrastructure;
using Alchemist.Test.Server.Fixtures;
using Grpc.Core;
using Xunit.Abstractions;

namespace Alchemist.Product.GrpcService.Tests;

public class AlchemistGrpcServiceIntegrationTest : TestFixture<AlchemistGrpcWebAppFactory, Program>
{
    private readonly AlchemyGrpcService.AlchemyGrpcServiceClient _client;

    public AlchemistGrpcServiceIntegrationTest(AlchemistGrpcWebAppFactory webAppFactory, ITestOutputHelper outputHelper) 
        : base(webAppFactory, outputHelper)
    {
        WebAppFactory.DataBase = "test_ci_db_grpc";
        var grpcChannel = webAppFactory.CreateChannel("http://localhost");
        _client = new AlchemyGrpcService.AlchemyGrpcServiceClient(grpcChannel);
    }

    [Fact]
    public async Task FindBrandSuccessWhenNameExists()
    {
        var brandName = "Elizavecca";        
        var response = await _client.FindBrandByNameAsync(new FindByNameRequest { Name = brandName});

        Assert.NotNull(response);
        Assert.Equal(brandName, response.Name);
    }

    [Fact]
    public async Task FindBrandByNameThrowsBadRequestRpcExceptionWhenNameIsEmpty()
    {
        var rpcException = await Assert.ThrowsAsync<RpcException>(async () =>
        {
            await _client.FindBrandByNameAsync(new FindByNameRequest { Name = "" });
        });

        OutputHelper.WriteLine(rpcException.Message);
        OutputHelper.WriteLine(rpcException.StackTrace);

        Assert.Equal(StatusCode.InvalidArgument, rpcException.Status.StatusCode);
    }

    [Fact]
    public async Task FindBrandByNameThrowsNotFoundRpcExceptionWhenNameNotExists()
    {
        var brandName = Guid.NewGuid().ToString();
        var rpcException = await Assert.ThrowsAsync<RpcException>(async () =>
        {
            await _client.FindBrandByNameAsync(new FindByNameRequest { Name = brandName });
        });

        OutputHelper.WriteLine(rpcException.Message);
        OutputHelper.WriteLine(rpcException.StackTrace);

        Assert.Equal(StatusCode.NotFound, rpcException.Status.StatusCode);
    }

    [Fact]
    public async Task CreateBrandSuccessWhenNameIsValid()
    {
        var brand = new CreateBrandRequest { Name = Guid.NewGuid().ToString() };
        var createResponse = await _client.CreateBrandAsync(brand);

        Assert.NotNull(createResponse);
        Assert.Equal(brand.Name, createResponse.Name);

        var findResponse = await _client.FindBrandByNameAsync(new FindByNameRequest { Name = brand.Name });
        Assert.NotNull(findResponse);
        Assert.Equal(createResponse.Id, findResponse.Id);
    }

    [Fact]
    public async Task CreateShopProductThrowsInternalRpcExceptionWhenShopIdOrProductIdAreNonUnique()
    {
        var createShopProductRequest = new CreateShopProductRequest
        {
            Apiurl = Guid.NewGuid().ToString(),
            Itemid = Guid.NewGuid().ToString(),
            Shopid = 1,
            Productid = 1,
            Itemurl = Guid.NewGuid().ToString(),
            Price = 0.0
        };
        var reply = await _client.CreateShopProductAsync(createShopProductRequest);
        
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
            await _client.CreateShopProductAsync(createShopProductRequestInvalid);
        });

        OutputHelper.WriteLine(rpcException.Message);
        OutputHelper.WriteLine(rpcException.StackTrace);

        Assert.Equal(StatusCode.Internal, rpcException.Status.StatusCode);
    }
}
