namespace Mediator.Messages;

public class Event<T>(string eventName, T entity, DateTime creationDate) : IEvent, IEventMessage
{
    public T Entity { get; } = entity;

    public DateTime CreationDate { get; } = creationDate;

    public string EventName { get; } = eventName;

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