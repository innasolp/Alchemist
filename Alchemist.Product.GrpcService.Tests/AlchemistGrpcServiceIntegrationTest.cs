using Alchemist.Product.GrpcService.Tests.Infrastructure;
using Alchemist.Test.Server.Fixtures;
using Grpc.Core;
using Xunit.Abstractions;

namespace Alchemist.Product.GrpcService.Tests;

public class TestAlchemistGrpcConfigurationWebAppFactory : AlchemistGrpcConfigurationPostgresWebAppFactory
{
    public TestAlchemistGrpcConfigurationWebAppFactory() : base("test_ci_db_grpc", 8074, 8075)
    {
    }
}

public class AlchemistGrpcServiceIntegrationTest : TestFixture<TestAlchemistGrpcConfigurationWebAppFactory, GrpcServiceProgramm>
{
    private readonly AlchemyGrpcService.AlchemyGrpcServiceClient _client;

    public AlchemistGrpcServiceIntegrationTest(TestAlchemistGrpcConfigurationWebAppFactory webAppFactory, ITestOutputHelper outputHelper)
        : base(webAppFactory, outputHelper)
    {
        var grpcChannel = webAppFactory.CreateChannel("http://localhost");
        _client = new AlchemyGrpcService.AlchemyGrpcServiceClient(grpcChannel);
    }

    [Fact]
    public async Task FindBrandSuccessWhenNameExists()
    {
        var brandName = "Elizavecca";

        try
        {
            var response = await _client.FindBrandByNameAsync(new FindByNameRequest { Name = brandName });

            Assert.NotNull(response);
            Assert.Equal(brandName, response.Name);
        }
        finally
        {
            await WebAppFactory.ResetDatabaseIfAvailableAsync();
        }
    }

    [Fact]
    public async Task FindBrandByNameThrowsBadRequestRpcExceptionWhenNameIsEmpty()
    {
        try
        {
            var rpcException = await Assert.ThrowsAsync<RpcException>(async () =>
            {
                await _client.FindBrandByNameAsync(new FindByNameRequest { Name = "" });
            });

            OutputHelper.WriteLine(rpcException.Message);
            OutputHelper.WriteLine(rpcException.StackTrace);

            Assert.Equal(StatusCode.InvalidArgument, rpcException.Status.StatusCode);
        }
        finally
        {
            await WebAppFactory.ResetDatabaseIfAvailableAsync();
        }
    }

    [Fact]
    public async Task FindBrandByNameThrowsNotFoundRpcExceptionWhenNameNotExists()
    {
        var brandName = Guid.NewGuid().ToString();

        try
        {
            var rpcException = await Assert.ThrowsAsync<RpcException>(async () =>
            {
                await _client.FindBrandByNameAsync(new FindByNameRequest { Name = brandName });
            });

            OutputHelper.WriteLine(rpcException.Message);
            OutputHelper.WriteLine(rpcException.StackTrace);

            Assert.Equal(StatusCode.NotFound, rpcException.Status.StatusCode);
        }
        finally
        {
            await WebAppFactory.ResetDatabaseIfAvailableAsync();
        }
    }

    [Fact]
    public async Task CreateBrandSuccessWhenNameIsValid()
    {
        var brand = new CreateBrandRequest { Name = Guid.NewGuid().ToString() };

        try
        {
            var createResponse = await _client.CreateBrandAsync(brand);

            Assert.NotNull(createResponse);
            Assert.Equal(brand.Name, createResponse.Name);

            var findResponse = await _client.FindBrandByNameAsync(new FindByNameRequest { Name = brand.Name });
            Assert.NotNull(findResponse);
            Assert.Equal(createResponse.Id, findResponse.Id);
        }
        finally
        {
            await WebAppFactory.ResetDatabaseIfAvailableAsync();
        }
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
        var createShopProductRequestInvalid = new CreateShopProductRequest
        {
            Apiurl = Guid.NewGuid().ToString(),
            Itemid = Guid.NewGuid().ToString(),
            Shopid = 1,
            Productid = 1,
            Itemurl = Guid.NewGuid().ToString(),
            Price = 0.0
        };

        try
        {
            var reply = await _client.CreateShopProductAsync(createShopProductRequest);

            var rpcException = await Assert.ThrowsAsync<RpcException>(async () =>
            {
                await _client.CreateShopProductAsync(createShopProductRequestInvalid);
            });

            OutputHelper.WriteLine(rpcException.Message);
            OutputHelper.WriteLine(rpcException.StackTrace);

            Assert.Equal(StatusCode.Internal, rpcException.Status.StatusCode);
        }
        finally
        {
            await WebAppFactory.ResetDatabaseIfAvailableAsync();
        }
    }
}
