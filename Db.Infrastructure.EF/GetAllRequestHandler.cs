using Db.Infrastructure.Requests;
using Microsoft.EntityFrameworkCore;

namespace Db.Infrastructure.EF;

public class GetAllRequestHandler<TRequest, T>(DbContext dbContext) : IRequestHandler<TRequest, IEnumerable<T>>
    where TRequest : GetAllRequest<T>
    where T:class
{
    public async Task<IEnumerable<T>> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<T>().ToListAsync(cancellationToken);
    }
}