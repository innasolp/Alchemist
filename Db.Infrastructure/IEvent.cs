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

public interface IIdentifiedEvent<T> : IEvent<T>
{
    string Id { get; }
}