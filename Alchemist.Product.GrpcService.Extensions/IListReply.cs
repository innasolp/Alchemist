using Google.Protobuf.Collections;
using Google.Protobuf;

namespace Alchemist.Product.GrpcService.Extensions;

public interface IListReply<T>
    where T : class, IMessage
{
    RepeatedField<T> Repeated { get; }
}
