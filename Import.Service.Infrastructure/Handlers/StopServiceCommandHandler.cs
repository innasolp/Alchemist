using Import.Service.Infrastructure;
using MediatR;
namespace Import.Service.Commands.Handlers;

internal sealed class StopServiceCommandHandler(IServiceRepository serviceRepository,
    IPublisher publisher) 
    : IRequestHandler<StopServiceCommand>
{
    private readonly IServiceRepository _serviceRepository = serviceRepository;

    private readonly IPublisher _publisher = publisher;

    public async Task Handle(StopServiceCommand request, CancellationToken cancellationToken)
    {
        var (service, stopTask) = _serviceRepository.StopServiceTask(request.Guid, cancellationToken);

        await stopTask;

        await _publisher.Publish(new ServiceStoppedEvent(new ServiceMessage(request.Guid, service.Name)), cancellationToken);
    }
}