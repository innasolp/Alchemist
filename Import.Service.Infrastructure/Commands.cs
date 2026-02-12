using Alchemist.Product.Entities;
using Import.Settings.Interfaces;
using MediatR;
using ShopSettings.Interfaces;

namespace Import.Service.Infrastructure;

public record AddImportServiceCommand(string Name, IImportSettings ImportSettings) : IRequest<Guid>;

public record StopServiceCommand(Guid Guid) : IRequest;

public record StartServiceCommand(Guid Guid) : IRequest;

public record StopAllServicesCommand : IRequest;

public record AddShopImportServiceFromShopSettingsCommand(IShopSettings ShopSettings) : IRequest<Guid>;

public record QueueShopCategoryToServicesCommand(ShopCategory ShopCategory) : IRequest;
