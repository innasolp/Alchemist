using Alchemist.Product.GrpcService.Tests.Infrastructure;
using Alchemist.Test.Log;
using Alchemist.Test.Server.Fixtures;
using Grpc.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Alchemist.Product.GrpcService.Tests;

public class AlchemistGrpcLoggingConfigurationWebAppFactory : AlchemistGrpcConfigurationPostgresWebAppFactory, ILoggedContext
{
    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    FixtureLogContext ILoggedContext.FixtureLoggingContext => FixtureLoggingContext;

    public AlchemistGrpcLoggingConfigurationWebAppFactory() : base("test_ci_db_grpc_logging", 8072, 8073)
    {
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        FixtureLoggingContext.ConfigureServices(services);
    }
}

public class AlchemistGrpcLoggingIntegrationTest : LoggedContextTestFixture<AlchemistGrpcLoggingConfigurationWebAppFactory, GrpcServiceProgramm>, IDisposable
{
    record TestLogMessage(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception);

    private readonly AlchemyGrpcService.AlchemyGrpcServiceClient _client;

    private readonly List<TestLogMessage> _messages = [];

    public AlchemistGrpcLoggingIntegrationTest(AlchemistGrpcLoggingConfigurationWebAppFactory webAppFactory, ITestOutputHelper outputHelper)
        : base(webAppFactory, outputHelper)
    {
        var grpcChannel = webAppFactory.CreateChannel("http://localhost");
        _client = new AlchemyGrpcService.AlchemyGrpcServiceClient(grpcChannel);

        WebAppFactory.FixtureLoggingContext.LoggedMessage += Log;
    }

    private void Log(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception)
    {
        _messages.Add(new TestLogMessage(logLevel, categoryName, eventId, message, exception));
    }

    [Fact]
    public async Task FindBrandByNameLogInfoSuccessWhenNameExists()
    {
        var brandName = "Elizavecca";
        _messages.Clear();

        try
        {
            var response = await _client.FindBrandByNameAsync(new FindByNameRequest { Name = brandName });

            Assert.NotNull(response);
            Assert.Equal(brandName, response.Name);

            Assert.Equal(1, _messages.Count(m =>
                m.logLevel == LogLevel.Information
                && m.message.Contains("HttpStatusCode: 200")
                && m.message.Contains(nameof(AlchemyGrpcService.AlchemyGrpcServiceClient.FindBrandByName))));
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }

    [Fact]
    public async Task FindBrandByNameLogErrorBadRequestWhenNameIsEmpty()
    {
        _messages.Clear();

        try
        {
            var rpcException = await Assert.ThrowsAsync<RpcException>(async () =>
            {
                await _client.FindBrandByNameAsync(new FindByNameRequest { Name = "" });
            });

            Assert.Equal(StatusCode.InvalidArgument, rpcException.Status.StatusCode);

            Assert.Equal(1, _messages.Count(m => m.logLevel == LogLevel.Error
                && m.message.Contains(nameof(AlchemyGrpcService.AlchemyGrpcServiceClient.FindBrandByName))));
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }

    [Fact]
    public async Task FindBrandByNameLogErrorNotFoundWhenNameNotExists()
    {
        _messages.Clear();

        try
        {
            var rpcException = await Assert.ThrowsAsync<RpcException>(async () =>
            {
                await _client.FindBrandByNameAsync(new FindByNameRequest { Name = Guid.NewGuid().ToString() });
            });

            Assert.Equal(StatusCode.NotFound, rpcException.Status.StatusCode);

            Assert.Equal(1, _messages.Count(m => m.logLevel == LogLevel.Error
                && m.message.Contains(nameof(AlchemyGrpcService.AlchemyGrpcServiceClient.FindBrandByName))));
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }

    [Fact]
    public async Task AddEqualShopProductsLogInternalServerError()
    {
        _messages.Clear();

        try
        {
            await _client.CreateShopProductAsync(new CreateShopProductRequest { Shopid = 1, Productid = 1, Itemurl = Guid.NewGuid().ToString() });

            var rpcException = await Assert.ThrowsAsync<RpcException>(async () =>
            {
                await _client.CreateShopProductAsync(new CreateShopProductRequest { Shopid = 1, Productid = 1, Itemurl = Guid.NewGuid().ToString() });
            });

            Assert.Equal(StatusCode.Internal, rpcException.Status.StatusCode);

            Assert.Equal(1, _messages.Count(m => m.logLevel == LogLevel.Error
                && m.message.Contains(nameof(AlchemyGrpcService.AlchemyGrpcServiceClient.CreateShopProduct))));
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }

    [Fact]
    public async Task FindBrandByNameLogWarningMutipleEntitesWhenNameDubles()
    {
        _messages.Clear();

        try
        {
            var result = await _client.FindBrandByNameAsync(new FindByNameRequest { Name = "infinite" });

            Assert.Equal(1, _messages.Count(m => m.logLevel == LogLevel.Warning
                && m.message.Contains(nameof(AlchemyGrpcService.AlchemyGrpcServiceClient.FindBrandByName))));
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }
}