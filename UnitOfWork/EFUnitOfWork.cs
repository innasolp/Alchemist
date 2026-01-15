using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace UnitOfWork;

public abstract class EFUnitOfWork<TDbContext>(TDbContext dbContext) : IUnitOfWork<IDbContextTransaction>
    where TDbContext : DbContext
{
    protected TDbContext Context { get; } = dbContext;    

    public virtual Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return  Context.Database.BeginTransactionAsync(cancellationToken);
    }

    public virtual Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
    {        
        return transaction.CommitAsync(cancellationToken)!;
    }

    public virtual Task RollbackTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
    {
        return transaction?.RollbackAsync(cancellationToken)!;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }
}