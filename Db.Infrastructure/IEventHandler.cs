namespace Db.Infrastructure;

public interface IEventHandler<TEvent> : INotificationHandler<TEvent> where TEvent : class, IEvent
{
}

public interface IEventHandler<T, TEvent> : INotificationHandler<TEvent> where TEvent : class, IEvent<T>
{
}