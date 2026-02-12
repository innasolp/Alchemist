using MediatR;
using UnitOfWork;

namespace Mediator.Infrastructure;

public abstract class TransactionCommandHandler<T, TCommand, TTransaction, TUnitOfWork>(TUnitOfWork unitOfWork)
    : IRequestHandler<TCommand, T>, ICommandHandler<TCommand, T>
    where TUnitOfWork : IUnitOfWork<TTransaction>
    where TCommand : Command<T>
{
    protected TUnitOfWork UnitOfWork { get; } = unitOfWork;

    public async Task<T> Handle(TCommand command, CancellationToken cancellationToken = default)
    {
        TTransaction? transaction = default;

        try
        {
            transaction = await UnitOfWork.BeginTransactionAsync(cancellationToken);

            var result = await HandleCommand(command, cancellationToken);

            await UnitOfWork.CommitTransactionAsync(transaction, cancellationToken);

            return result;
        }
        catch
        {
            await UnitOfWork.RollbackTransactionAsync(transaction!, cancellationToken);

            throw;
        }
    }
    internal protected abstract Task<T> HandleCommand(TCommand command, CancellationToken cancellationToken);    
}