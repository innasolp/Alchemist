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

        serviceItem.Service.ConnectedAsync += ServiceConnectedAsync;

        try
        {
            await _publisher.Publish(new ServiceStartingEvent(new ServiceMessage(request.Guid, serviceItem.Service.Name)), cancellationToken);

            await serviceItem.Service.Start(request.Guid, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, serviceItem.InnerTokenSource.Token).Token);
        }
        finally
        {
            serviceItem.Service.ConnectedAsync -= ServiceConnectedAsync;
        }
    }

    private async Task ServiceConnectedAsync(object sender, ConnectedAsyncEventArgs eventArgs)
    {
        if (sender is not IImportService service || eventArgs.Parameter is not Guid guid) return;

        await _publisher.Publish(new ServiceStartedEvent(eventArgs.Success, new ServiceMessage(guid, service.Name)), eventArgs.CancellationToken);        

        service.ConnectedAsync -= ServiceConnectedAsync;
    }
}