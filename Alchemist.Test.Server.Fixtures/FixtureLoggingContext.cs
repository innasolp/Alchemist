using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alchemist.Test.Server.Fixtures;

public class FixtureLoggingContext : IDisposable
{
    public event LogMessage? LoggedMessage;

    public LoggerFactory LoggerFactory { get; }

    public FixtureLoggingContext()
    {
        LoggerFactory = new LoggerFactory();
        LoggerFactory.AddProvider(new ForwardingLoggerProvider((logLevel, category, eventId, message, exception) =>
        {
            LoggedMessage?.Invoke(logLevel, category, eventId, message, exception);
        }));
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var loggerFactoryService = services.SingleOrDefault(s => s.ServiceType == typeof(ILoggerFactory));
        if (loggerFactoryService != null)
            services.Remove(loggerFactoryService);

        services.AddSingleton<ILoggerFactory>(LoggerFactory);
    }

    public void Dispose()
    {
        LoggerFactory.Dispose();
    }
}
