using Microsoft.Extensions.Logging;

namespace Alchemist.Test.Log;

public delegate void LogMessage(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception);

public partial class ForwardingLoggerProvider(LogMessage logAction) : ILoggerProvider
{
    private readonly LogMessage _logAction = logAction;

    public ILogger CreateLogger(string categoryName)
    {
        return new ForwardingLogger(categoryName, _logAction);
    }

    public void Dispose()
    {
    }
}
