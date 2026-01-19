using Mediator.Infrastructure.Request;
using MediatR;
using UnitOfWork;

namespace Mediator.Infrastructure;

public class GetAllRequestHandler<TRequest, T, TRepository>(TRepository repository) : IRequestHandler<TRequest, List<T>>
    where TRequest : GetAllRequest<T>
    where TRepository : IRepository<T>
{
    protected TRepository Repository { get; } = repository;

    public Task<List<T>> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        return Repository.GetAll(cancellationToken);
    }
}

public class GetAllRequestHandler<TRequest, T>(IRepository<T> repository) : GetAllRequestHandler<TRequest, T, IRepository<T>>(repository)
    where TRequest : GetAllRequest<T>
{
}