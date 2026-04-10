using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Xunit.Abstractions;

namespace Alchemist.Test.Server.Fixtures;

public record TestLogMessage(LogLevel LogLevel, string CategoryName, EventId EventId, string Message, Exception? Exception);

public abstract class LoggedContextTest(ITestOutputHelper outputHelper)
{
    protected ITestOutputHelper OutputHelper { get; private set; } = outputHelper;

    private readonly BlockingCollection<TestLogMessage> _logMessages = [];

    protected IEnumerable<TestLogMessage> LogMessages => _logMessages;

    private readonly Semaphore _semaphore = new(1, 1);

    protected void Log(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception)
    {
        _semaphore.WaitOne();
        _logMessages.Add(new TestLogMessage(logLevel, categoryName, eventId, message, exception));
        _semaphore.Release();
    }

    protected void OutputErrors()
    {
        var errors = _logMessages.Where(m => m.LogLevel == LogLevel.Error);
        if (!errors.Any()) return;

        OutputHelper.WriteLine("Errors:");

        foreach (var error in errors)
        {
            OutputHelper.WriteLine($"{error.Message} : {error.Exception?.Message ?? ""}");
            if (error.Exception != null)
                OutputHelper.WriteLine(error.Exception.StackTrace);

            if(error.Exception?.InnerException != null)
            {
                OutputHelper.WriteLine("Inner exception :");
                OutputHelper.WriteLine(error.Exception.InnerException.Message);
                OutputHelper.WriteLine(error.Exception.InnerException.StackTrace);
            }
        }
    }

    protected void OutputWarnings()
    {
        var warnings = _logMessages.Where(m => m.LogLevel == LogLevel.Warning).ToArray();
        if (warnings.Length == 0) return;
        OutputHelper.WriteLine("Warnings:");
        foreach (var warning in warnings)
        {
            OutputHelper.WriteLine($"{warning.Message} : {warning.Exception?.Message ?? ""}");
            if (warning.Exception != null)
                OutputHelper.WriteLine(warning.Exception.StackTrace);
        }
    }
}
