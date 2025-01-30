using Google.Protobuf;
using Google.Protobuf.Collections;
using Alchemist.Product.GrpcService.Extensions;

namespace Alchemist.Product.GrpcService;

public interface IListReply<T>
    where T : class, IMessage
{
    RepeatedField<T> Repeated { get; }
}

//todo example
//public sealed partial class ProductTypeListReply : IListReply<ProductTypeReply>
//{
//    RepeatedField<ProductTypeReply> IListReply<ProductTypeReply>.Repeated => ProductTypes;
//}

public partial class ProductTypeReply : IBaseReply<int> { }

public partial class PurposeTypeReply : IBaseReply<int> { }

public partial class CountryReply : IBaseReply<int> { }


public partial class BrandReply : IBaseReply<int> { }

public partial class ComponentReply : IBaseReply<int>, IComponentMessage { }

public partial class ProductReply : IBaseReply<long> { }

public partial class CreateShopProductRequest : IShopProductMessage { }

public partial class ShopProductReply : IShopProductMessage { }

public partial class CreateShopProductRequest : IShopProductMessage { }

public partial class UpdateShopProductRequest : IShopProductMessage { }


public partial class CreateProductRequest : IProductMessage { }
public partial class ProductReply : IProductMessage { }

public partial class ShopProductPriceReply : IShopProductPriceMessage { }

public partial class CreateShopProductPriceRequest : IShopProductPriceMessage { }

public partial class UpdateShopProductPriceRequest : IShopProductPriceMessage { }

public partial class CreateComponentRequest: IComponentMessage { }

public partial class SetProductComponentRequest : IProductComponentMessage { }

public partial class ProductComponentReply : IProductComponentMessage { }

public partial class CreateCurrencyRequest : ICurrencyMessage { }

public partial class CurrencyReply : ICurrencyMessage { }

    public static class ServiceExtensions
{
    public static Task<TListReply> GetListReply<TListReply, TReply, TEntity>(this
        List<TEntity> entities,
        Func<TEntity, TReply> createReplyItem)
        where TListReply : class, IListReply<TReply>, IMessage, new()
        where TReply : class, IMessage, new()
    {
        var list = entities.Select(item => createReplyItem(item)).ToList();
        var listReply = new TListReply();
        listReply.Repeated.AddRange(list);
        return Task.FromResult(listReply);
    }
}
