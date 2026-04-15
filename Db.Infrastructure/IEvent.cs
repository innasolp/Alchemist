namespace Db.Infrastructure;

public interface IEvent
{
    DateTime CreationDate { get; }

    string EventName { get; }
}

public interface IEvent<T> : IEvent
{
    T Entity { get; }
}