using Microsoft.Extensions.Logging;

namespace Alchemist.Test.Server.Fixtures;

public class ForwardingLogger(string categoryName, LogMessage logAction) : ILogger
{
    private readonly string _categoryName = categoryName;
    private readonly LogMessage _logAction = logAction;

    public IDisposable BeginScope<TState>(TState state)
    {
        return null!;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _logAction(logLevel, _categoryName, eventId, formatter(state, exception), exception);
    }
}

public class ForwardingLogger<T>(LogMessage logAction) : ForwardingLogger(typeof(T).Name, logAction), ILogger<T>
{ }

