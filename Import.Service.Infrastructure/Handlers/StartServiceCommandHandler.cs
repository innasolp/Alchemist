using Import.Interfaces;
using MediatR;

namespace Import.Service.Infrastructure.Handlers;

internal sealed class StartServiceCommandHandler(IServiceManager serviceRepository,
    IPublisher publisher)
    : IRequestHandler<StartServiceCommand>
{
    private readonly IServiceManager _serviceRepository = serviceRepository;

    private readonly IPublisher _publisher = publisher;

    public async Task Handle(StartServiceCommand request, CancellationToken cancellationToken)
    {
        async Task serviceConnectedAsync(object sender, ConnectedAsyncEventArgs eventArgs)
        {
            if (sender is not IImportService service) return;

            try
            {
                await _publisher.Publish(new ServiceStartedEvent(eventArgs.Connected, new ServiceMessage(request.Guid, service.Name)), eventArgs.CancellationToken);
            }
            finally
            {
                service.ConnectedAsync -= serviceConnectedAsync;
            }
        }

        var (service, startTask) = _serviceRepository.StartServiceTask(request.Guid, cancellationToken);

        service.ConnectedAsync += serviceConnectedAsync;

        await _publisher.Publish(new ServiceStartingEvent(new ServiceMessage(request.Guid, service.Name)), cancellationToken);

        try
        {
            await startTask;
        }
        catch
        {
            service.ConnectedAsync -= serviceConnectedAsync;
            throw;
        }
    }
}