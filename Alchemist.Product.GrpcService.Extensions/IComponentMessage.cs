namespace Alchemist.Product.GrpcService.Extensions;

public interface IComponentMessage
{
    string Name { get; set; }
    string Description { get; set; } 
    int? Groupid { get; set; }
    string Transcript { get; set; }
}
