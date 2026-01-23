using Alchemist.Import.Settings;
using Alchemist.Product.Entities;
using MediatR;
using ShopSettings.Interfaces;

namespace Import.Service.Commands;

public record AddShopImportServiceCommand(string Name, IShopImportSettings ImportSettings) : IRequest<Guid>;

public record AddShopImportServiceFromShopSettingsCommand(IShopSettings ShopSettings) : IRequest<Guid>;

public record StopServiceCommand(Guid Guid) : IRequest;

public record StartServiceCommand(Guid Guid) : IRequest;

public record StopAllServicesCommand : IRequest;

public record QueueShopCategoryToServicesCommand(ShopCategory ShopCategory) : IRequest;
