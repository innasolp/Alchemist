using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
using Grpc.Core;

namespace Alchemist.Product.GrpcService.Extensions;

public static class GrpcStatuses
{
    public static Grpc.Core.Status GetBadRequestRpcStatus(string paramName)
    {
        BadRequest badRequest = new();
        badRequest.FieldViolations.Add(new BadRequest.Types.FieldViolation { Field = paramName, Description = "Value is null or empty" });

        return new Grpc.Core.Status(StatusCode.InvalidArgument, Any.Pack(badRequest).Value.ToBase64());   
    }

    public static RpcException GetBadRequestRpcException(string paramName)
    {
        var status = new Google.Rpc.Status() { Code = (int)StatusCode.InvalidArgument };
        status.Details.Add(Any.Pack(new BadRequest
        {
            FieldViolations =
                        {
                            new BadRequest.Types.FieldViolation { Field = paramName, Description = "Value is null or empty" }
                        }
        }));
        return status.ToRpcException();
    }

    public static RpcException GetBadRequestRpcException(string paramName, string message)
    {
        var status = new Google.Rpc.Status() { Code = (int)StatusCode.InvalidArgument };
        status.Details.Add(Any.Pack(new BadRequest
        {
            FieldViolations =
                        {
                            new BadRequest.Types.FieldViolation { Field = paramName, Description = message }
                        }
        }));
        return status.ToRpcException();
    }
}
