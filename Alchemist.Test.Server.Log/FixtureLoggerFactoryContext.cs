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
        var loggerFactoryServices = services.Where(s => s.ServiceType == typeof(ILoggerFactory)).ToList();
        if (loggerFactoryServices.Count > 0)
            loggerFactoryServices.ForEach(s=>services.Remove(s));            

        services.AddSingleton<ILoggerFactory>(LoggerFactory);
    }    

    public void Dispose()
    {
        LoggerFactory.Dispose();
    }
}


