using Mediator.Infrastructure.Request;
using MediatR;
using UnitOfWork;

namespace Mediator.Infrastructure;

public class FindByNamePropertyRequestHandler<T, TId, TRequest, TRepository>(TRepository repository) : IRequestHandler<TRequest, T>
    where TRequest : FindByNamePropertyRequest<T>
    where TRepository : IRepository<T>
    where TId : struct
{
    protected TRepository Repository { get; } = repository;

    public virtual Task<T> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        return Repository.FindByName(request.Name, request.NameProperties, cancellationToken)!;
    }
}