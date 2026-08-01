using Alchemist.Product.Entities;
using Db.Infrastructure;
using Import.Settings.Interfaces;
using ShopSettings.Interfaces;

namespace Import.Service.Infrastructure;

public record AddImportServiceCommand(string Name, IImportSettings ImportSettings) : ICommand;

public record StopServiceCommand(Guid Guid) : ICommand;

public record StartServiceCommand(Guid Guid) : ICommand;

public record StopAllServicesCommand : ICommand;

public record AddShopImportServiceFromShopSettingsCommand(IShopSettings ShopSettings) : ICommand;

public record QueueShopCategoryToServicesCommand(ShopCategory ShopCategory) : ICommand;
