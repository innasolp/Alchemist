namespace Mediator.Infrastructure.Events;

public class UpdateEvent<T>(string eventName, T entity, DateTime creationDate) : Event<T>(eventName, entity, creationDate)
{
}