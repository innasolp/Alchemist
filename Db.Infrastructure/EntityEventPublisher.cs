using Microsoft.Extensions.DependencyInjection;

namespace Db.Infrastructure;

public class EntityEventPublisher(IServiceProvider serviceProvider) : IEntityEventPublisher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task Publish<T,TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class, IEvent<T>
    {
        ArgumentNullException.ThrowIfNull(@event);

        var handlers = _serviceProvider.GetServices<IEventHandler<T,TEvent>>();

        foreach (var handler in handlers)
        {
            await handler.Handle(@event, cancellationToken);
        }
    }
}