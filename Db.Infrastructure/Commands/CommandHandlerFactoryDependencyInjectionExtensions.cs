using Microsoft.Extensions.DependencyInjection;

namespace Db.Infrastructure.Commands;

public static class CommandHandlerFactoryDependencyInjectionExtensions
{
    public static IServiceCollection AddCommandHandlerFactory(this IServiceCollection services)
    {
        return services.AddScoped<ICommandHandlerFactory, CommandHandlerFactory>();
    }
}