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
                await PublishConnectedEvent(request.Guid, service, eventArgs.Connected, eventArgs.CancellationToken);
            }
            finally
            {
                service.ConnectedAsync -= serviceConnectedAsync;
            }
        }

        var (connecting, service, startTask) = _serviceRepository.StartService(request.Guid, cancellationToken);

        if (!service.Connected)
        {
            service.ConnectedAsync += serviceConnectedAsync;

            await _publisher.Publish(new ServiceStartingEvent(new ServiceMessage(request.Guid, service.Name)), cancellationToken);

            if(!connecting && startTask != null)
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
        else
        {
            await PublishConnectedEvent(request.Guid, service, true, cancellationToken);

            if(!connecting && startTask != null)
                 await startTask;
        }        
    }

    private Task PublishConnectedEvent(Guid id, IImportService service, bool connected, CancellationToken cancellationToken)
    {
        return _publisher.Publish(new ServiceStartedEvent(connected, new ServiceMessage(id, service.Name)), cancellationToken);
    }
}