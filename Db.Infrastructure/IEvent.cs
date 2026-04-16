namespace Db.Infrastructure;

public interface IEvent : INotification
{
    DateTime CreationDate { get; }

    string EventName { get; }
}

public interface IEvent<T> : IEvent
{
    T Entity { get; }
}