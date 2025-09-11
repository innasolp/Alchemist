using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Alchemist.Test.Server.Fixtures;

public record TestLogMessage(LogLevel LogLevel, string CategoryName, EventId EventId, string Message, Exception? Exception);

public abstract class LogContextTestFixture<TWebAppFactory, TEntryPoint> : TestFixture<TWebAppFactory, TEntryPoint>
     where TEntryPoint : class
    where TWebAppFactory : WebApplicationFactory<TEntryPoint>, ILoggedContext
{
    private readonly List<TestLogMessage> _logMessages = [];

    public LogContextTestFixture(TWebAppFactory webAppFactory, ITestOutputHelper outputHelper) : base(webAppFactory, outputHelper)
    {
        WebAppFactory.FixtureLoggingContext.LoggedMessage += Log;
    }

    protected IEnumerable<TestLogMessage> LogMessages => _logMessages;

    protected void Log(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception)
    {
        _logMessages.Add(new TestLogMessage(logLevel, categoryName, eventId, message, exception));
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
        }
    }

    protected void OutputWarnings()
    {
        var warnings = _logMessages.Where(m => m.LogLevel == LogLevel.Warning);
        if (!warnings.Any()) return;
        OutputHelper.WriteLine("Warnings:");
        foreach (var warning in warnings)
        {
            OutputHelper.WriteLine($"{warning.Message} : {warning.Exception?.Message ?? ""}");
            if (warning.Exception != null)
                OutputHelper.WriteLine(warning.Exception.StackTrace);
        }
    }
}
