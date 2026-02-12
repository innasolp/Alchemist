namespace UnitOfWork;

public interface IUnitOfWork<TTransaction>
{
    Task<TTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(TTransaction transaction, CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(TTransaction transaction, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}