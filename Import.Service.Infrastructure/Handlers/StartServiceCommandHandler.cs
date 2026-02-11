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

        IImportService? service = null;

        try
        {
            (service, var startTask) = _serviceRepository.StartServiceTask(request.Guid, cancellationToken);
            using(startTask)
            service.ConnectedAsync += serviceConnectedAsync;

            await _publisher.Publish(new ServiceStartingEvent(new ServiceMessage(request.Guid, service.Name)), cancellationToken);            
            
            await startTask;
        }
        catch
        {
            if(service != null) service.ConnectedAsync -= serviceConnectedAsync;

            throw;
        }
    }
}