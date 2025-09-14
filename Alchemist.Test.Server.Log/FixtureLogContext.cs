using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alchemist.Test.Log;

public abstract class FixtureLogContext 
{
    public event LogMessage? LoggedMessage;
    protected void InvokeLogMessage(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception)
    {
        LoggedMessage?.Invoke(logLevel, categoryName, eventId, message, exception);
    }
    public abstract void ConfigureServices(IServiceCollection services);
}
