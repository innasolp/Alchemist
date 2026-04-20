using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Db.Infrastructure.EF.Outbox;

public static class AppExtensions
{
    public static async Task UseOutbox<TContext>(this IHost app, CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await context.CreateMessageEntryTableIfNotExistsAsync(cancellationToken).ConfigureAwait(false);

            await context.CreateMessageHandlerTableIfNotExistsAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
        }
    }
}