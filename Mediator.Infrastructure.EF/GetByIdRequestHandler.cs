using Mediator.Infrastructure.Request;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Mediator.Infrastructure.EF;

public abstract class GetByIdRequestHandler<T, TId, TRequest, TDbContext> (TDbContext dbContext) : IRequestHandler<TRequest, T>
     where T : class
    where TRequest : GetByIdRequest<TId, T>
    where TDbContext : DbContext
    where TId:struct
{
    protected TDbContext DbContext { get; } = dbContext;

    public virtual Task<T> Handle(TRequest request, CancellationToken cancellationToken  =default)
    {
        return DbContext.GetById<T>(request.Id, cancellationToken)!;
    }
}

public class GetByIdRequestHandler<TRequest, T, TDbContext>(TDbContext dbContext) : IRequestHandler<TRequest, T>
     where T : class
     where TRequest : GetByIdRequest<T>
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext = dbContext;

    public Task<T> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        return _dbContext.GetById<T>(request.Id, cancellationToken)!;
    }
}

public class GetByIdRequestHandler<TRequest, T>(DbContext dbContext) : GetByIdRequestHandler<TRequest, T, DbContext>(dbContext)
     where T : class
     where TRequest : GetByIdRequest<T>
{
}