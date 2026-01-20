namespace Import.Service.Commands;

public class Event(string eventName, DateTime creationDate) : IEvent
{   
    public DateTime CreationDate { get; } = creationDate;

    public string EventName { get; } = eventName;
}

public class Event<T>(string eventName, T message, DateTime creationDate) : Event(eventName, creationDate), IEvent<T>
{
    public T Message { get; } = message;
}