using Mediator.Infrastructure.Request;
using MediatR;
using UnitOfWork;

namespace Mediator.Infrastructure;

public abstract class FindByNameRequestHandler<T, TRequest, TRepository>(TRepository repository) : IRequestHandler<TRequest, T>
    where TRequest : FindByNameRequest<T>
    where TRepository : IRepository<T>
{
    protected TRepository Repository { get; } = repository;

    public virtual Task<T> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        return Repository.FindByName(request.GetName, request.Name, cancellationToken)!;
    }
}

public class FindByNameRequestHandler<TRequest,T>(IRepository<T> repository) 
    : FindByNameRequestHandler<T, TRequest, IRepository<T>>(repository)
    where TRequest : FindByNameRequest<T>
{
}