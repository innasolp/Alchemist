using Db.Infrastructure.Requests;
using Microsoft.EntityFrameworkCore;

namespace Db.Infrastructure.EF;

public class FindByNameRequestHandler<TRequest,T>(DbContext dbContext) : IRequestHandler<TRequest, T>
    where T:class
    where TRequest : FindByNameRequest<T>
{
    public virtual async Task<T?> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        var allEntities = await dbContext.Set<T>().ToListAsync(cancellationToken);

        var formattedName = request.Name.Trim().ToUpper();

        //todo stringcomparision
        var entities = allEntities.Where(e => request.GetName(e).Trim().Equals(formattedName, StringComparison.CurrentCultureIgnoreCase))
            .ToList();

        return entities.Count > 1
            ? throw new EntityWarningException($"multiple entities with name {request.Name}", entities.FirstOrDefault())
            : entities.FirstOrDefault();
    }
}