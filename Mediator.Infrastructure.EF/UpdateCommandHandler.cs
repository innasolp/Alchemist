using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Mediator.Infrastructure.EF;

public class UpdateCommandHandler<T, TUpdateCommand, TDbContext, TUnitOfWork>(TDbContext dbContext,TUnitOfWork unitOfWork)
    : TransactionCommandHandler<T, TUpdateCommand, IDbContextTransaction, TUnitOfWork>(unitOfWork), IUpdateCommandHandler<TUpdateCommand, T>
    where T:class
    where TDbContext : DbContext
    where TUnitOfWork : EFUnitOfWork<TDbContext>
    where TUpdateCommand : UpdateCommand<T>
{
    protected TDbContext DbContext { get; } = dbContext;

    protected override async Task<T> HandleCommand(TUpdateCommand request, CancellationToken cancellationToken)
    {
        var result = DbContext.Update(request.Entity);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return result.Entity;
    }
}

public class UpdateCommandHandler<T, TUpdateCommand>(DbContext dbContext, EFUnitOfWork<DbContext> unitOfWork) 
    : UpdateCommandHandler<T, TUpdateCommand, DbContext, EFUnitOfWork<DbContext>>(dbContext, unitOfWork)
     where T : class
     where TUpdateCommand : UpdateCommand<T>
{}