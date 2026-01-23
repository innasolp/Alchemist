using Mediator.Messages;

namespace Mediator.Messages.Events;

public class CreationEvent<T>(string eventName, T entity, DateTime creationDate) : Event<T>(eventName, entity, creationDate)
{
}
