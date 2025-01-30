namespace Alchemist.Product.GrpcService.Extensions;

public  interface IProductComponentMessage
{
    int Componentid { get; set; }

    long Productid { get; set; }

    int SequalNumber { get; set; }
}
