using Db.Infrastructure;

namespace Import.Service.Infrastructure.Handlers;

internal sealed class StopAllServicesCommandHandler(IServiceManager serviceRepository,
    IEntityEventPublisher publisher) :
    ICommandHandler<StopAllServicesCommand>
{
    private readonly IServiceManager _serviceRepository = serviceRepository;

    private readonly IEntityEventPublisher _publisher = publisher;

    public async Task Handle(StopAllServicesCommand request, CancellationToken cancellationToken = default)
    {
        var stopServiceTasks = _serviceRepository.StopAllServicesTask(cancellationToken).ToList();

        await Task.WhenAll(stopServiceTasks.Select(async st=>
        {
            await st.stopTask;
            await _publisher.Publish<ServiceMessage, ServiceStoppedEvent>(new ServiceStoppedEvent(new ServiceMessage(st.guid, st.service.Name)), cancellationToken);
        }));
    }
}