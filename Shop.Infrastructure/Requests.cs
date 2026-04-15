using Alchemist.Product.Data;
using MediatR;

namespace Shop.Infrastructure;

public record GetShopByUrlRequest(string Url) : IRequest<Alchemist.Product.Data.Shop>;

public record GetShopCategoriesRequest(int ShopId) : IRequest<List<ShopCategory>>;

public record GetAllCategoryChildrenRequest(int ParentId) : IRequest<List<ShopCategory>>;

public record GetShopCategoryByShopIdAndItemIdRequest(int ShopId, int ItemId) : IRequest<ShopCategory>;

public record CheckCategoryForAncestorItemrRequest(int Id, int AncestorItemId) : IRequest<bool?>;