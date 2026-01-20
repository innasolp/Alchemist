using MediatR;

namespace Import.Service.Commands.Handlers;

internal sealed class StopAllServicesCommandHandler(IServiceRepository serviceRepository,
    IPublisher publisher) :
    IRequestHandler<StopAllServicesCommand>
{
    private readonly IServiceRepository _serviceRepository = serviceRepository;

    private readonly IPublisher _publisher = publisher;

    public Task Handle(StopAllServicesCommand request, CancellationToken cancellationToken = default)
    {
        return Task.WhenAll(_serviceRepository.Services.Select( s =>StopServiceAsync(s.Key, cancellationToken)));
    }

    private async Task StopServiceAsync(Guid guid, CancellationToken cancellationToken)
    {
        if (!_serviceRepository.Services.TryGetValue(guid, out var serviceItem))
            throw new InvalidOperationException($"Service with id {guid} not found.");

        await serviceItem.InnerTokenSource.CancelAsync();

        await _publisher.Publish(new ServiceStoppedEvent(new ServiceMessage(guid, serviceItem.Service.Name)), cancellationToken);
    }
}