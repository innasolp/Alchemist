namespace Db.Infrastructure;

public class Event<T>(T entity, string eventName, DateTime creationDate) : IEvent<T>
{
    public T Entity => entity;

    public DateTime CreationDate => creationDate;

    public string EventName => eventName;
}