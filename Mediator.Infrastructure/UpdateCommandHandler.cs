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

    protected override async Task<T> HandlerRequest(TUpdateCommand request, CancellationToken cancellationToken)
    {
        var result = await Repository.Update(request.Entity, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}