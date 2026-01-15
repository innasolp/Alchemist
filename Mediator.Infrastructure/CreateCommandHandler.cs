using Mediator.Infrastructure.Command;
using UnitOfWork;

namespace Mediator.Infrastructure;

public abstract class CreateCommandHandler<T, TCreateRequest, TRepository, TTransaction, TUnitOfWork>(TRepository repository, TUnitOfWork unitOfWork) 
    : CommandHandler<T, TCreateRequest, TTransaction, TUnitOfWork>(unitOfWork)
    where TRepository : IRepository<T>
    where TUnitOfWork : IUnitOfWork<TTransaction>
    where TCreateRequest : CreateCommand<T>
{
    protected TRepository Repository { get; } = repository;

    protected override Task<T> HandlerRequest(TCreateRequest request, CancellationToken cancellationToken)
    {
        return Repository.Create(request.Entity, cancellationToken);
    }
}