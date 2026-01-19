using Mediator.Infrastructure.Request;
using MediatR;
using UnitOfWork;

namespace Mediator.Infrastructure;

public abstract class GetByIdRequestHandler<T, TId, TRequest, TRepository > (TRepository repository) : IRequestHandler<TRequest, T>
    where TRequest : GetByIdRequest<TId, T>
    where TRepository : IRepository<T>
    where TId:struct
{
    protected TRepository Repository { get; } = repository;

    public virtual Task<T> Handle(TRequest request, CancellationToken cancellationToken  =default)
    {
        return Repository.GetById(request.Id, cancellationToken)!;
    }
}

public class GetByIdRequestHandler<TRequest, T>(IRepository<T> repository) : IRequestHandler<TRequest, T>
     where TRequest : GetByIdRequest<T>
{
    private readonly IRepository<T> _repository = repository;

    public Task<T> Handle(TRequest request, CancellationToken cancellationToken = default)
    {
        return _repository.GetById(request.Id, cancellationToken)!;
    }
}