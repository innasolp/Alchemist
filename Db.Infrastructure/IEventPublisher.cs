namespace Db.Infrastructure;

public interface IEventPublisher
{
    Task Publish<TEvent>(TEvent notification, CancellationToken cancellationToken = default)
        where TEvent : IEvent;
}

public interface IEntityEventPublisher
{
    Task Publish<T, TEvent>(TEvent notification, CancellationToken cancellationToken = default)
        where TEvent : IEvent<T>;
}