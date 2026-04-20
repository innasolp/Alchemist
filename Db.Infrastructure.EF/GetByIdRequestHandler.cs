using Db.Infrastructure.Requests;
using Microsoft.EntityFrameworkCore;

namespace Db.Infrastructure.EF;

public class GetByIdRequestHandler<TRequest,T, TId>(DbContext dbContext) : IRequestHandler<TRequest, T?>
    where TRequest : GetByIdRequest<TId, T>
    where TId:struct
    where T:class
{
    public virtual async Task<T?> Handle(TRequest request, CancellationToken cancellationToken  =default)
    {
        return await dbContext.Set<T>().FindAsync(request.Id, cancellationToken);
    }
}

public class GetByIdRequestHandler<T>(DbContext dbContext) : IRequestHandler<GetByIdRequest<T>, T?>
    where T : class
{
    public virtual async Task<T?> Handle(GetByIdRequest<T> request, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<T>().FindAsync(request.Id, cancellationToken);
    }
}