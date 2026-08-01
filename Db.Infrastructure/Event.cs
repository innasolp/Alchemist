namespace Db.Infrastructure;

public class Event(string eventName, DateTime creationDate) : IEvent
{
    public DateTime CreationDate => creationDate;

    public string EventName => eventName;
}

public class Event<T>(T entity, string eventName, DateTime creationDate) : Event(eventName, creationDate), IEvent<T>
{
    public T Entity => entity;
}