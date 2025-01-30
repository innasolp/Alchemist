namespace Alchemist.Product.GrpcService.Extensions;

public interface IBaseReply<TId>
    where TId : struct
{
    string Name { get; set; }

    TId Id { get; set; }
}
