using Mediator.Infrastructure.Command;
using UnitOfWork;

namespace Mediator.Infrastructure;

public abstract class CreateCommandHandler<T, TCreateRequest, TRepository, TTransaction, TUnitOfWork>(TRepository repository, TUnitOfWork unitOfWork) 
    : TransactionCommandHandler<T, TCreateRequest, TTransaction, TUnitOfWork>(unitOfWork)
    where TRepository : IRepository<T>
    where TUnitOfWork : IUnitOfWork<TTransaction>
    where TCreateRequest : CreateCommand<T>
{
    protected TRepository Repository { get; } = repository;

    protected override async Task<T> HandlerRequest(TCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await Repository.Create(request.Entity, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}