using MediatR;
using UnitOfWork;

namespace Mediator.Infrastructure;

public abstract class CommandHandler<T, TCommand, TTransaction, TUnitOfWork>(TUnitOfWork unitOfWork)
    : IRequestHandler<TCommand, T>    
    where TUnitOfWork : IUnitOfWork<TTransaction>
    where TCommand : IRequest<T>
{
    protected TUnitOfWork UnitOfWork { get; } = unitOfWork;

    public async Task<T> Handle(TCommand request, CancellationToken cancellationToken = default)
    {
        TTransaction? transaction = default;

        try
        {
            transaction = await UnitOfWork.BeginTransactionAsync(cancellationToken);

            var result = await HandlerRequest(request, cancellationToken);

            await UnitOfWork.CommitTransactionAsync(transaction, cancellationToken);

            return result;
        }
        catch
        {
            await UnitOfWork.RollbackTransactionAsync(transaction!, cancellationToken);

            throw;
        }
    }

    protected abstract Task<T> HandlerRequest(TCommand request, CancellationToken cancellationToken);    
}