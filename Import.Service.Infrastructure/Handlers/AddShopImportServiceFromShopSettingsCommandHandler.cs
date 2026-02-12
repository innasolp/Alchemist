using Alchemist.Import.Settings.DataAdapter;
using Import.Service.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ShopSettings.Interfaces;

namespace Import.Service.Commands.Handlers;

internal sealed class AddShopImportServiceFromShopSettingsCommandHandler(IServiceRepository serviceRepository,
    [FromKeyedServices(ServiceKeys.ProcessedImportSettings)] IDictionary<ShopSettingType, ISettingsDataAdapter> dataAdapters,
    IPublisher publisher) 
    : IRequestHandler<AddShopImportServiceFromShopSettingsCommand, Guid>
{
    private readonly IServiceRepository _serviceRepository = serviceRepository;

    private readonly IDictionary<ShopSettingType, ISettingsDataAdapter> _dataAdapters = dataAdapters;

    private readonly IPublisher _publisher = publisher;

    public async Task<Guid> Handle(AddShopImportServiceFromShopSettingsCommand request, CancellationToken cancellationToken)
    {
        if (!_dataAdapters.TryGetValue(request.ShopSettings.Type, out var adapter))
            return Guid.Empty;

        var shopImportSettings = await adapter.GetShopImportSettings(request.ShopSettings.Id, cancellationToken);

        var (guid, service) = await _serviceRepository.AddImportService(request.ShopSettings.Name, shopImportSettings, cancellationToken);

        if (service is not null)
            await _publisher.Publish(new ServiceCreatedEvent(new ServiceMessage(guid, service.Name)), cancellationToken);

        return guid;
    }
}