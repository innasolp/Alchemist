using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alchemist.DependencyInjection.Common;

public static class LogExtensions
{
    public static IServiceCollection AddLogger(this IServiceCollection services, string categoryName)
    {
        return services.AddScoped(serviceProvider =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            return loggerFactory.CreateLogger(categoryName);
        });
    }
}