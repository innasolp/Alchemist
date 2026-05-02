using Mediator.Infrastructure;


namespace Import.Service.Infrastructure;

public record ServiceMessage(Guid Guid, string? Name);

public record ServiceStartedMessage(bool Success, Guid Guid, string? Name) : ServiceMessage(Guid, Name);

public abstract class ServiceEvent(string eventName, ServiceMessage serviceMessage, DateTime creationDate) 
    : Event<ServiceMessage>(eventName, serviceMessage, creationDate)
{
    protected override (string, object?[]) GetFailedMessage()
    {
        return ("Notification event {EventName} for service {Entity.Name} {Entity.Guid} failed.", [EventName, Entity.Name, Entity.Guid]);
    }

    protected override (string, object?[]) GetSuccessEventMessage()
    {
        return ("Successfully published event {EventName} for service {Entity.Name} {Entity.Guid}.", [EventName, Entity.Name, Entity.Guid]);
    }
}

public class ServiceCreatedEvent(ServiceMessage message) : ServiceEvent(Messages.ServiceCreated, message, DateTime.Now);

public class ServiceStartingEvent(ServiceMessage message) : ServiceEvent(Messages.ServiceStarting, message, DateTime.Now);

public class ServiceStartedEvent(bool success, ServiceMessage message) : Event<ServiceStartedMessage>(Messages.ServiceStarted, 
    new ServiceStartedMessage(success, message.Guid, message.Name), DateTime.Now);

public class ServiceStoppedEvent(ServiceMessage message) : ServiceEvent(Messages.ServiceStopped, message, DateTime.Now);