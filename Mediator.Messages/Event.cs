namespace Mediator.Messages;

public class Event<T>(string eventName, T entity, DateTime creationDate) : IEvent
{
    public T Entity { get; } = entity;

    public DateTime CreationDate { get; } = creationDate;

    public string EventName { get; } = eventName;
}