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
        if (!_serviceRepository.Services.TryGetValue(request.Guid, out var serviceItem))
            throw new InvalidOperationException($"Service with id {request.Guid} not found.");

        await serviceItem.InnerTokenSource.CancelAsync();

        await _publisher.Publish(new ServiceStoppedEvent(new ServiceMessage(request.Guid, serviceItem.Service.Name)), cancellationToken);
    }
}