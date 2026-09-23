using Db.Infrastructure;

namespace Import.Service.Infrastructure.Handlers;

public sealed class AddShopImportServiceCommandHandler(IServiceManager serviceRepository,
    IEntityEventPublisher publisher) 
    : ICommandHandler<AddImportServiceCommand, Guid>
{
    private readonly IServiceManager _serviceRepository = serviceRepository;
    
    private readonly IEntityEventPublisher _publisher = publisher;

    public async Task<Guid> Handle(AddImportServiceCommand request, CancellationToken cancellationToken = default)
    {
        var (guid, service) = await _serviceRepository.AddImportService(request.Name, request.ImportSettings, cancellationToken);

        if(service is not null)
            await _publisher.Publish<ServiceMessage, ServiceCreatedEvent>(new ServiceCreatedEvent(new ServiceMessage(guid, service.Name)), cancellationToken);

        return  guid;
    }
}