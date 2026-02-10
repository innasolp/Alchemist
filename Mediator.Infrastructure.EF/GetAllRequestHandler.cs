using Mediator.Infrastructure.Request;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mediator.Infrastructure.EF;

public class GetAllRequestHandler<TRequest, T, TDbContext>(TDbContext dbContext) : IRequestHandler<TRequest, List<T>>
    where TRequest : GetAllRequest<T>
    where TDbContext : DbContext
    where T:class
{
    protected TDbContext DbContext { get; } = dbContext;

    public Task<List<T>> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        return DbContext.Set<T>().ToListAsync(cancellationToken);
    }
}

public class GetAllRequestHandler<TRequest, T>(DbContext dbContext) : GetAllRequestHandler<TRequest, T, DbContext>(dbContext)
    where T : class
    where TRequest : GetAllRequest<T>
{}