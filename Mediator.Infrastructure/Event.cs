namespace Mediator.Infrastructure;

public class Event<T>() : IEvent, IEventMessage
{
    public T Entity { get; init; }

    public DateTime CreationDate { get; init; }

    public string EventName { get; init; }

    public Event(string eventName, T entity, DateTime creationDate) : this()
    {
        Entity = entity;
        CreationDate = creationDate;
        EventName = eventName;
    }

    protected internal virtual (string, object?[]) GetSuccessEventMessage()
    {
        return ("Successfully published event {EventName} for {Entity}.", [EventName, Entity]);
    }

    protected internal virtual (string, object?[]) GetFailedMessage()
    {
        return ("Notification event {EventName} for {Entity} failed.", [EventName, Entity]);
    }

    (string, object?[]) IEventMessage.GetSuccessEventMessage()
    {
        return GetSuccessEventMessage();
    }

    (string, object?[]) IEventMessage.GetFailedMessage()
    {
        return GetFailedMessage();
    }
}