using Import.Service.Infrastructure;
using MediatR;

namespace Import.Service.Commands.Handlers;

internal sealed class StopAllServicesCommandHandler(IServiceRepository serviceRepository,
    IPublisher publisher) :
    IRequestHandler<StopAllServicesCommand>
{
    private readonly IServiceRepository _serviceRepository = serviceRepository;

    private readonly IPublisher _publisher = publisher;

    public async Task Handle(StopAllServicesCommand request, CancellationToken cancellationToken = default)
    {
        var stopServiceTasks = _serviceRepository.StopAllServicesTask(cancellationToken).ToList();

        await Task.WhenAll(stopServiceTasks.Select(async st=>
        {
            await st.stopTask;
            await _publisher.Publish(new ServiceStoppedEvent(new ServiceMessage(st.guid, st.service.Name)), cancellationToken);
        }));
    }
}