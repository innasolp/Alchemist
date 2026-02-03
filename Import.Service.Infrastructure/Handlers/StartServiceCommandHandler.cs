using Import.Interfaces;
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

        async Task serviceConnectedAsync(object sender, ConnectedAsyncEventArgs eventArgs)
        {
            if (sender is not IImportService service) return;

            try
            {
                await _publisher.Publish(new ServiceStartedEvent(eventArgs.Success, new ServiceMessage(request.Guid, service.Name)), eventArgs.CancellationToken);
            }
            finally
            {
                service.ConnectedAsync -= serviceConnectedAsync;
            }
        }

        serviceItem.Service.ConnectedAsync += serviceConnectedAsync;

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, serviceItem.InnerTokenSource.Token);

        try
        {
            await _publisher.Publish(new ServiceStartingEvent(new ServiceMessage(request.Guid, serviceItem.Service.Name)), cancellationToken);

            await serviceItem.Service.Start(linkedCts.Token);
        }
        catch
        {
            serviceItem.Service.ConnectedAsync -= serviceConnectedAsync;

            throw;
        }
    }
}