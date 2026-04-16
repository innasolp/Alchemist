using Db.Infrastructure.Requests;
using Microsoft.EntityFrameworkCore;

namespace Db.Infrastructure.EF;

public abstract class GetByIdRequestHandler<TRequest,T, TId>(DbContext dbContext) : IRequestHandler<TRequest, T>
    where TRequest : GetByIdRequest<TId, T>
    where TId:struct
    where T:class
{
    public virtual async Task<T?> Handle(TRequest request, CancellationToken cancellationToken  =default)
    {
        return await dbContext.Set<T>().FindAsync(request.Id, cancellationToken);
    }
}