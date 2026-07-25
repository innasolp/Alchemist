using Data.Extensions;
using Db.Infrastructure.Requests;
using Microsoft.EntityFrameworkCore;

namespace Db.Infrastructure.EF;

public class FindByNameRequestHandler<T>(DbContext dbContext) : IRequestHandler<FindByNameRequest<T>, T?>
    where T : class, IEntity
{
    public async Task<T?> Handle(FindByNameRequest<T> request, CancellationToken cancellationToken)
    {
        var formattedName = request.Name.Trim().ToUpper();

        //todo stringcomparision
        var entities = await dbContext.Set<T>().Where(e => e.Name.Trim().ToUpper() == formattedName).ToListAsync();

        return entities.Count > 1
            ? throw new EntityWarningException($"multiple entities with name {request.Name}", entities.FirstOrDefault())
            : entities.FirstOrDefault();
    }
}