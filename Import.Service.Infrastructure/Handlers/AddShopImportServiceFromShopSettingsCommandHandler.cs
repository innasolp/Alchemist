using Alchemist.Import.Settings.DataAdapter;
using Db.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using ShopSettings.Interfaces;

namespace Import.Service.Infrastructure.Handlers;

internal sealed class AddShopImportServiceFromShopSettingsCommandHandler(IServiceManager serviceRepository,
    [FromKeyedServices(ServiceKeys.ProcessedImportSettings)] IDictionary<ShopSettingType, ISettingsDataAdapter> dataAdapters,
    IEntityEventPublisher publisher) 
    : ICommandHandler<AddShopImportServiceFromShopSettingsCommand, Guid>
{
    private readonly IServiceManager _serviceRepository = serviceRepository;

    private readonly IDictionary<ShopSettingType, ISettingsDataAdapter> _dataAdapters = dataAdapters;

    private readonly IEntityEventPublisher _publisher = publisher;

    public async Task<Guid> Handle(AddShopImportServiceFromShopSettingsCommand request, CancellationToken cancellationToken = default)
    {
        if (!_dataAdapters.TryGetValue(request.ShopSettings.Type, out var adapter))
            return Guid.Empty;

        var shopImportSettings = await adapter.GetShopImportSettings(request.ShopSettings.Id, cancellationToken);

        var (guid, service) = await _serviceRepository.AddImportService(request.ShopSettings.Name, shopImportSettings, cancellationToken);

        if (service is not null)
            await _publisher.Publish<ServiceMessage, ServiceCreatedEvent>(new ServiceCreatedEvent(new ServiceMessage(guid, service.Name)), cancellationToken);

        return guid;
    }
}