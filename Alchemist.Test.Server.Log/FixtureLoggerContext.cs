using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alchemist.Test.Log;

public class FixtureLoggerContext<TCategory> : FixtureLoggerFactoryContext
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var logger = new ForwardingLogger<TCategory>(InvokeLogMessage);
        var loggerDescriptor = services.SingleOrDefault(s => s.ServiceType == typeof(ILogger<TCategory>));
        if (loggerDescriptor != null)
            services.Remove(loggerDescriptor);
        services.AddSingleton<ILogger<TCategory>>(logger);
    }
}
