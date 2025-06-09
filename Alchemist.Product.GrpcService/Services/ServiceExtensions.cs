using Google.Protobuf.Collections;
using Alchemist.Product.GrpcService.Extensions;
using Grpc.Interfaces;
using Grpc.Message.Extensions;

namespace Alchemist.Product.GrpcService;

public partial class ProductTypeReply : IBaseReply<int>, IMessageWarning { }

public partial class PurposeTypeReply : IBaseReply<int>, IMessageWarning { }

public partial class CountryReply : IBaseReply<int>, IMessageWarning { }


public partial class BrandReply : IBaseReply<int>, IMessageWarning { }

public partial class ComponentReply : IBaseReply<int>, IComponentMessage { }

public partial class ProductReply : IBaseReply<long>, IProductMessage, IMessageWarning { }

public partial class CreateShopProductRequest : IShopProductMessage { }

public partial class ShopProductReply : IShopProductMessage { }

public partial class CreateShopProductRequest : IShopProductMessage { }

public partial class UpdateShopProductRequest : IShopProductMessage { }


public partial class CreateProductRequest : IProductMessage { }

public partial class ShopProductPriceReply : IShopProductPriceMessage { }

public partial class CreateShopProductPriceRequest : IShopProductPriceMessage { }

public partial class UpdateShopProductPriceRequest : IShopProductPriceMessage { }

public partial class ShopProductCategoryRequest : IShopProductCategoryMessage { }

public partial class ShopProductCategoryReply : IShopProductCategoryMessage { }

public partial class ShopProductCategoryListReply: IListReply<ShopProductCategoryReply>
{
    RepeatedField<ShopProductCategoryReply> IListReply<ShopProductCategoryReply>.Repeated => ShopProductCategories;
}

public partial class PurposeTypeListReply : IListReply<PurposeTypeReply>
{
    RepeatedField<PurposeTypeReply> IListReply<PurposeTypeReply>.Repeated => PurposeTypes;
}

public partial class CreateComponentRequest: IComponentMessage { }

public partial class SetProductComponentRequest : IProductComponentMessage { }

public partial class ProductComponentReply : IProductComponentMessage { }

public partial class CreateCurrencyRequest : ICurrencyMessage { }

public partial class CurrencyReply : ICurrencyMessage, IMessageWarning { }

