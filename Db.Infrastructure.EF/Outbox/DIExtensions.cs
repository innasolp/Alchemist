using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Db.Infrastructure.EF.Outbox;

public static class DIExtensions
{
    public static IServiceCollection AddOutboxProcessor<TDbContext>(this IServiceCollection services)
        where TDbContext:DbContext
    {
        services.AddScoped<IOutboxEventPublisher, OutboxEntityEventPublisher>();
        return services.AddHostedService<OutboxBackgroundService<TDbContext>>();
    }
}