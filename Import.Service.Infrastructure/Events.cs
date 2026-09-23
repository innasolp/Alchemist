using Db.Infrastructure;

namespace Import.Service.Infrastructure;

public record ServiceMessage(Guid Guid, string? Name);

public record ServiceStartedMessage(bool Success, Guid Guid, string? Name) : ServiceMessage(Guid, Name);

public abstract class ServiceEvent(string eventName, ServiceMessage serviceMessage, DateTime creationDate) 
    : Event<ServiceMessage>(serviceMessage, eventName, creationDate)
{
}

public class ServiceCreatedEvent(ServiceMessage message) : ServiceEvent(Messages.ServiceCreated, message, DateTime.Now);

public class ServiceStartingEvent(ServiceMessage message) : ServiceEvent(Messages.ServiceStarting, message, DateTime.Now);

public class ServiceStartedEvent(bool success, ServiceMessage message) : Event<ServiceStartedMessage>( 
    new ServiceStartedMessage(success, message.Guid, message.Name), Messages.ServiceStarted,DateTime.Now);

public class ServiceStoppedEvent(ServiceMessage message) : ServiceEvent(Messages.ServiceStopped, message, DateTime.Now);