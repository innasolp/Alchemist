namespace Db.Infrastructure;

public abstract class TransactionalCommandHandler<TCommand>(IUnitOfWork unitOfWork)
    : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;

    public async Task Handle(TCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            await UnitOfWork.BeginTransactionAsync(cancellationToken);

            await HandleCommand(command, cancellationToken);

            await UnitOfWork.SaveChangesAsync(cancellationToken);

            await UnitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await UnitOfWork.RollbackTransactionAsync(cancellationToken);

            throw;
        }
    }

    protected abstract Task HandleCommand(TCommand command, CancellationToken cancellationToken);    
}