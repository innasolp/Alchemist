using Google.Protobuf.WellKnownTypes;

namespace Alchemist.Product.GrpcService.Extensions;

public interface IProductMessage
{
    string Name { get; set; }
    int Producttypeid { get; set; }
    int? Brandid { get; set; }
    int? Initshopid { get; set; }
    string Articul { get; set; }
    string Transcript { get; set; }
}
