using MediatR;

namespace Import.Service.Commands.Handlers;

public sealed class AddShopImportServiceCommandHandler(IServiceRepository serviceRepository, 
    IPublisher publisher) 
    : IRequestHandler<AddImportServiceCommand, Guid>
{
    private readonly IServiceRepository _serviceRepository = serviceRepository;
    private readonly IPublisher _publisher = publisher;

    public async Task<Guid> Handle(AddImportServiceCommand request, CancellationToken cancellationToken = default)
    {
        var (guid, service) = await _serviceRepository.AddImportService(request.Name, request.ImportSettings, cancellationToken);

        if(service is not null)
            await _publisher.Publish(new ServiceCreatedEvent(new ServiceMessage(guid, service.Name)), cancellationToken);

        return  guid;
    }
}