namespace Alchemist.Product.GrpcService.Extensions;

public  interface ICurrencyMessage
{
    string Name { get; set; }
    string? Fullname { get; set; }
    int? Code { get; set; }
}
