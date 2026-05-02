using Mediator.Infrastructure.Request;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mediator.Infrastructure.EF;

public class FindByNameRequestHandler<T, TRequest, TDbContext>(TDbContext dbContext) : IRequestHandler<TRequest, T>
    where T:class
    where TRequest : FindByNameRequest<T>
    where TDbContext : DbContext
{
    protected TDbContext DbContext { get; } = dbContext;

    public virtual Task<T> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        return DbContext.FindByName(request.GetName, request.Name, cancellationToken)!;
    }
}

public class FindByNameRequestHandler<T, TRequest>(DbContext dbContext) : FindByNameRequestHandler<T, TRequest, DbContext>(dbContext)
    where T : class
    where TRequest : FindByNameRequest<T>
{ }