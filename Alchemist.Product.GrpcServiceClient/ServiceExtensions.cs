using Google.Protobuf.Collections;
using GrpcExtensions.Message;

namespace Alchemist.Product.GrpcService;

public partial class ShopProductCategoryListReply : IListReply<ShopProductCategoryReply>
{
    RepeatedField<ShopProductCategoryReply> IListReply<ShopProductCategoryReply>.Repeated => ShopProductCategories;
}

public partial class PurposeTypeListReply : IListReply<PurposeTypeReply>
{
    RepeatedField<PurposeTypeReply> IListReply<PurposeTypeReply>.Repeated => PurposeTypes;
}