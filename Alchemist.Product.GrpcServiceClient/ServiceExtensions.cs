using Alchemist.Product.GrpcService.Extensions;
using Google.Protobuf.Collections;

namespace Alchemist.Product.GrpcService;

public partial class CreateShopProductRequest : IShopProductMessage { }

public partial class UpdateShopProductRequest : IShopProductMessage { }

public partial class CreateProductRequest : IProductMessage { }

public partial class ShopProductReply : IShopProductMessage { }

public partial class ProductReply : IProductMessage { }

public partial class CreateComponentRequest : IComponentMessage { }

public partial class ComponentReply : IComponentMessage { }

public partial class CreateShopProductPriceRequest : IShopProductPriceMessage { }

public partial class UpdateShopProductPriceRequest : IShopProductPriceMessage { }

public partial class ShopProductPriceReply: IShopProductPriceMessage { }

public partial class ShopProductCategoryRequest : IShopProductCategoryMessage { }

public partial class ShopProductCategoryReply : IShopProductCategoryMessage { }

public partial class ShopProductCategoryListReply : IListReply<ShopProductCategoryReply>
{
    RepeatedField<ShopProductCategoryReply> IListReply<ShopProductCategoryReply>.Repeated => ShopProductCategories;
}

public partial class SetProductComponentRequest : IProductComponentMessage { }

public partial class ProductComponentReply : IProductComponentMessage { }

public partial class CreateCurrencyRequest : ICurrencyMessage { }

public partial class CurrencyReply : ICurrencyMessage { }

