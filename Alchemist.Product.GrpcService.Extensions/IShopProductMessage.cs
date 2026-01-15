namespace Alchemist.Product.GrpcService.Extensions;

public interface IShopProductMessage
{
    int Shopid { get; set; }
    long Productid { get; set; }
    string Itemid { get; set; }
    string Apiurl { get; set; }
    string Itemurl { get; set; }
    bool? Isactual { get; set; }
    double? Price { get; set; }
}
