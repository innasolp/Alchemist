using Microsoft.AspNetCore.SignalR;

namespace Alchemist.Product.SignalR;

public class LogHubFilter (ILogger<LogHubFilter> logger) : IHubFilter
{
    private readonly ILogger<LogHubFilter> _logger = logger;

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        _logger.LogInformation($"Start calling method {invocationContext.HubMethodName} with  parameters [{invocationContext.HubMethodArguments}] of hub {invocationContext.Context.ConnectionId}.");
        try
        {
            var result = await next(invocationContext);   
            _logger.LogInformation($"Method {invocationContext.HubMethodName} with  parameters [{invocationContext.HubMethodArguments}] of hub {invocationContext.Context.ConnectionId} completed successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Method {invocationContext.HubMethodName} with  parameters [{invocationContext.HubMethodArguments}]  of hub {invocationContext.Context.ConnectionId} failed: {ex.Message}");
            throw;
        }
    }
}
