using Alchemist.Test.Server.Fixtures.Grpc;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Alchemist.Product.GrpcService.Tests;

public class AlchemistGrpcLoggingIntegrationTest : GrpcTestFixture<AlchemistGrpcLoggingWebAppFactory, Program>, IDisposable
{
    record TestLogMessage (LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception);

    private readonly AlchemyGrpcService.AlchemyGrpcServiceClient _client;
    
    private readonly List<TestLogMessage> _messages = [];

    public AlchemistGrpcLoggingIntegrationTest(AlchemistGrpcLoggingWebAppFactory webAppFactory, ITestOutputHelper outputHelper) 
        : base(webAppFactory, outputHelper)
    {
        _client = new AlchemyGrpcService.AlchemyGrpcServiceClient(GrpcChannel);

        WebAppFactory.FixtureLoggingContext.LoggedMessage += Log;
    }

    private void Log(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception)
    {
        _messages.Add(new TestLogMessage(logLevel, categoryName, eventId, message, exception));
    }

    [Fact]
    public async Task LogFindBrandByNameSuccess()
    {
        var brandName = "Elizavecca";
        _messages.Clear();

        var response = await _client.FindBrandByNameAsync(new FindByNameRequest { Name = brandName });

        Assert.NotNull(response);
        Assert.Equal(brandName, response.Name);

        Assert.Equal(2, _messages.Count(m=>m.eventId == 5001 &&
            m.logLevel == LogLevel.Information
            && m.message.Contains(nameof(AlchemyGrpcService.AlchemyGrpcServiceClient.FindBrandByName))));
    }

    [Fact]
    public async Task LogFindBrandByNameThrowsException()
    {
        _messages.Clear();

        var rpcException = await Assert.ThrowsAsync<RpcException>(async () =>
        {
            await _client.FindBrandByNameAsync(new FindByNameRequest { Name = "" });
        });       

        Assert.Equal(StatusCode.InvalidArgument, rpcException.Status.StatusCode);
        
        Assert.Equal(2, _messages.Count(m => m.eventId == 5001 &&
            m.logLevel == LogLevel.Information
            && m.message.Contains(nameof(AlchemyGrpcService.AlchemyGrpcServiceClient.FindBrandByName))));
        
        Assert.Equal(1, _messages.Count(m=> m.logLevel == LogLevel.Error
            && m.message.Contains(nameof(AlchemyGrpcService.AlchemyGrpcServiceClient.FindBrandByName))));
    }

    public void Dispose()
    {
        WebAppFactory.FixtureLoggingContext.LoggedMessage -= Log;
        base.Dispose();
    }
}
