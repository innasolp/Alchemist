using MediatR;

namespace Import.Service.Commands.Handlers;

internal sealed class AddShopImportServiceCommandHandler(IServiceRepository serviceRepository, 
    IPublisher publisher) 
    : IRequestHandler<AddShopImportServiceCommand, (bool, Guid)>
{
    private readonly IServiceRepository _serviceRepository = serviceRepository;
    private readonly IPublisher _publisher = publisher;

    public async Task<(bool, Guid)> Handle(AddShopImportServiceCommand request, CancellationToken cancellationToken = default)
    {
        var (success, guid, service) = await _serviceRepository.TryAddImportService(request.Name, request.ImportSettings, cancellationToken);

        if(service is not null)
            await _publisher.Publish(new ServiceCreatedEvent(new ServiceMessage(guid, service.Name)), cancellationToken);

        return  (success, guid);
    }
}