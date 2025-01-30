using Google.Protobuf.WellKnownTypes;

namespace Alchemist.Product.GrpcService.Extensions;

public interface IShopProductPriceMessage
{
    int Currencyid {  get; set; }

    long Shopproductid { get; set; }

    double Price { get; set; }

    Timestamp Lastupdate { get; set; }
}
