namespace Mediator.Infrastructure.Events;

public class CreationEvent<T> : Event<T>
{
    public CreationEvent()
    {
    }

    public CreationEvent(string eventName, T entity, DateTime creationDate) : base(eventName, entity, creationDate)
    {
    }
}