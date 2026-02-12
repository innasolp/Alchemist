namespace Mediator.Infrastructure.Events;

public class UpdateEvent<T> : Event<T>
{
    public UpdateEvent(string eventName, T entity, DateTime creationDate) : base(eventName, entity, creationDate)
    {
    }

    public UpdateEvent()
    {
    }
}