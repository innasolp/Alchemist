using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alchemist.Test.Log;

public class FixtureLoggerFactoryContext : FixtureLogContext, IDisposable
{ 
    public LoggerFactory LoggerFactory { get; }

    public FixtureLoggerFactoryContext()
    {
        LoggerFactory = new LoggerFactory();
        LoggerFactory.AddProvider(new ForwardingLoggerProvider(InvokeLogMessage));
    }

    public override void ConfigureServices(IServiceCollection services)
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


