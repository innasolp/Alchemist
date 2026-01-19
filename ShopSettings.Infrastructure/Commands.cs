using MediatR;

namespace ShopSettings.Infrastructure;

public record SaveShopSettingsCommand(Alchemist.Product.Data.ShopSettings ShopSettings) : IRequest<Alchemist.Product.Data.ShopSettings>;

public record SaveShopSettingsWithChildrenCommand(Alchemist.Product.Data.ShopSettings ParentShopSettings, 
    IEnumerable<Alchemist.Product.Data.ShopSettings> ChildrenSettings)
    : IRequest<List<Alchemist.Product.Data.ShopSettings>>;