using Alchemist.Product.Data;
using Db.Infrastructure;

namespace ShopSettings.Data.Infrastructure;

public record GetShopSettingsByShopIdRequest(int ShopId, ShopSettingType SettingType) : IRequest<Alchemist.Product.Data.ShopSettings?>;

public record GetChildSettingsRequest(int ParentSettingsId) : IRequest<List<Alchemist.Product.Data.ShopSettings>>;

public record GetAllParentShopSettingsRequest() : IRequest<List<Alchemist.Product.Data.ShopSettings>>;
