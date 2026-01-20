using MediatR;

namespace Import.Service.Commands.Handlers;

internal sealed class StartServiceCommandHandler(IServiceRepository serviceRepository,
    IPublisher publisher)
    : IRequestHandler<StartServiceCommand>
{
    private readonly IServiceRepository _serviceRepository = serviceRepository;

    private readonly IPublisher _publisher = publisher;

    public async Task Handle(StartServiceCommand request, CancellationToken cancellationToken)
    {
        if (!_serviceRepository.Services.TryGetValue(request.Guid, out var serviceItem))
            throw new InvalidOperationException($"Service with id {request.Guid} not found.");

        await serviceItem.Service.Start(CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, serviceItem.InnerTokenSource.Token).Token);

        await _publisher.Publish(new ServiceStartedEvent(new ServiceMessage(request.Guid, serviceItem.Service.Name)), cancellationToken);
    }
}