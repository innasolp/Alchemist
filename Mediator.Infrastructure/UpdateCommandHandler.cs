using Mediator.Infrastructure.Command;
using UnitOfWork;

namespace Mediator.Infrastructure;

public class UpdateCommandHandler<T, TUpdateCommand, TRepository, TTransaction, TUnitOfWork>(TRepository repository, TUnitOfWork unitOfWork) 
    : CommandHandler<T, TUpdateCommand, TTransaction, TUnitOfWork>(unitOfWork)
    where TRepository : IRepository<T>
    where TUnitOfWork : IUnitOfWork<TTransaction>
    where TUpdateCommand : UpdateCommand<T>
{
    protected TRepository Repository { get; } = repository;

    protected override Task<T> HandlerRequest(TUpdateCommand request, CancellationToken cancellationToken)
    {
        return Repository.Update(request.Entity, cancellationToken);
    }
}