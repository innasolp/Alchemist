using Microsoft.Extensions.Logging;

namespace Alchemist.Test.Server.Fixtures;

public delegate void LogMessage(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception);

public class ForwardingLoggerProvider(LogMessage logAction) : ILoggerProvider
{
    private readonly LogMessage _logAction = logAction;

    public ILogger CreateLogger(string categoryName)
    {
        return new ForwardingLogger(categoryName, _logAction);
    }

    public void Dispose()
    {
    }

    internal class ForwardingLogger(string categoryName, LogMessage logAction) : ILogger
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
}
