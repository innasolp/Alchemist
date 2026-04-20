using Db.Infrastructure;

namespace ShopSettings.Data.Infrastructure;

public sealed record SaveShopSettingsCommand(Alchemist.Product.Data.ShopSettings ShopSettings) : ICommand;

public sealed record SaveShopSettingsWithChildrenCommand(Alchemist.Product.Data.ShopSettings ParentShopSettings, IEnumerable<Alchemist.Product.Data.ShopSettings> ChildrenSettings) : ICommand;
