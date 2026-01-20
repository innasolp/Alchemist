using MediatR;

namespace Import.Service.Commands.Handlers;

internal sealed class StartAllServicesCommandHandler(IServiceRepository serviceRepository,
    IPublisher publisher) :
    IRequestHandler<StartAllServicesCommand>
{
    private readonly IServiceRepository _serviceRepository = serviceRepository;

    private readonly IPublisher _publisher = publisher;

    public Task Handle(StartAllServicesCommand request, CancellationToken cancellationToken = default)
    {
        return Parallel.ForEachAsync(_serviceRepository.Services, (s, t) =>
                new ValueTask(StartServiceAsync(s.Key, cancellationToken)));
    }

    private async Task StartServiceAsync(Guid guid, CancellationToken cancellationToken)
    {
        if (!_serviceRepository.Services.TryGetValue(guid, out var serviceItem))
            throw new InvalidOperationException($"Service with id {guid} not found.");

        await serviceItem.Service.Start(CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, serviceItem.InnerTokenSource.Token).Token);

        await _publisher.Publish(new ServiceStartedEvent(new ServiceMessage(guid, serviceItem.Service.Name)), cancellationToken);
    }
}