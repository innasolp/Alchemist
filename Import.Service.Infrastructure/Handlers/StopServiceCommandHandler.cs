using Db.Infrastructure;

namespace Import.Service.Infrastructure.Handlers;

internal sealed class StopServiceCommandHandler(IServiceManager serviceRepository,
    IEntityEventPublisher publisher) 
    : ICommandHandler<StopServiceCommand>
{
    private readonly IServiceManager _serviceRepository = serviceRepository;

    private readonly IEntityEventPublisher _publisher = publisher;

    public async Task Handle(StopServiceCommand request, CancellationToken cancellationToken)
    {
        var (service, stopTask) = _serviceRepository.StopServiceTask(request.Guid, cancellationToken);

        await stopTask;

        await _publisher.Publish<ServiceMessage, ServiceStoppedEvent>(new ServiceStoppedEvent(new ServiceMessage(request.Guid, service.Name)), cancellationToken);
    }
}