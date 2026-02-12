using Mediator.Infrastructure.Request;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mediator.Infrastructure.EF;

public class FindByNamePropertyRequestHandler<T, TId, TRequest, TDbContext>(TDbContext dbContext) : IRequestHandler<TRequest, T>
    where T:class
    where TRequest : FindByNamePropertyRequest<T>
    where TDbContext : DbContext
    where TId : struct
{
    protected TDbContext DbContext { get; } = dbContext;

    public virtual Task<T> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        return DbContext.FindByName(request.Name, request.NameProperties, cancellationToken)!;
    }
}