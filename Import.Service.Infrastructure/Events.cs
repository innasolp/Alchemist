using Mediator.Messages;

namespace Import.Service.Commands;

public record ServiceMessage(Guid Guid, string? Name);

public class ServiceCreatedEvent(ServiceMessage message) : Event<ServiceMessage>(Messages.ServiceCreated, message, DateTime.Now);
public class ServiceStartedEvent(ServiceMessage message) : Event<ServiceMessage>(Messages.ServiceStarted, message, DateTime.Now);
public class ServiceStoppedEvent(ServiceMessage message) : Event<ServiceMessage>(Messages.ServiceStopped, message, DateTime.Now);