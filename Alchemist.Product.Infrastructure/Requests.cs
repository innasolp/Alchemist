using Alchemist.Product.Data;
using MediatR;

namespace Alchemist.Product.Infrastructure;

public record CheckShopProductCategoryRequest(long ShopProductId, int ShopCategoryId) : IRequest<bool>;

public record GetShopProductCategoriesRequest(long ShopId) : IRequest<List<ShopProductCategory>>;

public record FindProductByNameAndBrandRequest(string Name, string Brand):IRequest<Data.Product>;

public record GetShopProductByShopAndItemUrlRequest(int ShopId, string ItemUrl) : IRequest<ShopProduct>;

public record GetShopProductByShopAndItemIdRequest(int ShopId, string ItemId) : IRequest<ShopProduct>;

public record GetShopProductByShopAndProductIdRequest(int ShopId, long ProductId) : IRequest<ShopProduct>;

public record GetCurrencyByCodeRequest(short Code) : IRequest<Currency>;

public record GetProductPurposeTypesRequest(long ProductId) : IRequest<List<PurposeType>>;