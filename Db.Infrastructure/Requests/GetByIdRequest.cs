namespace Db.Infrastructure.Requests;

public class GetByIdRequest<TId, T>(TId id) : IRequest<T>
    where TId: struct
{
    public TId Id { get; } = id;
}

public class GetByIdRequest<T>(object id) : IRequest<T>
{
    public object Id { get; } = id;
}