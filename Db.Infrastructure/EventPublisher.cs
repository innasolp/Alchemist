using Microsoft.Extensions.DependencyInjection;

namespace Db.Infrastructure;

public class EventPublisher(IServiceProvider serviceProvider) : IEventPublisher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(@event);

        var handlers = _serviceProvider.GetServices<IEventHandler<TEvent>>();

        foreach (var handler in handlers)
        {
            await handler.Handle(@event, cancellationToken);
        }
    }
}