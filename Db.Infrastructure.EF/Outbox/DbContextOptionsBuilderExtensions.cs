using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Db.Infrastructure.EF.Outbox;

public static class DbContextOptionsBuilderExtensions
{
    public static void AddOutbox(this  DbContextOptionsBuilder optionsBuilder, IServiceProvider serviceProvider,
        IReadOnlyList<SupportedEventType> supportedTypes)
    {
        var entityEventPublisher = serviceProvider.GetRequiredService<IEntityEventPublisher>();

        optionsBuilder.AddInterceptors(new OutboxSaveChangesInterceptor(supportedTypes, entityEventPublisher));
    }
}