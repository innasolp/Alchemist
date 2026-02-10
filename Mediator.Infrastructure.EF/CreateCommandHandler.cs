using Mediator.Infrastructure.Command;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Mediator.Infrastructure.EF;

public class CreateCommandHandler<T, TCreateCommand, TDbContext, TUnitOfWork>(TDbContext dbContext, TUnitOfWork unitOfWork) 
    : TransactionCommandHandler<T, TCreateCommand, IDbContextTransaction, TUnitOfWork>(unitOfWork), ICreateCommandHandler<TCreateCommand,T>
    where T:class
    where TDbContext : DbContext
    where TUnitOfWork : EFUnitOfWork<TDbContext>
    where TCreateCommand : CreateCommand<T>
{
    protected TDbContext DbContext { get; } = dbContext;

    protected override async Task<T> HandleCommand(TCreateCommand request, CancellationToken cancellationToken)
    {
        var result = await DbContext.Create(request.Entity, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
        return result;
    }
}

public class CreateCommandHandler<T, TCreateRequest>(DbContext dbContext, EFUnitOfWork<DbContext> unitOfWork) 
    : CreateCommandHandler<T, TCreateRequest, DbContext, EFUnitOfWork<DbContext>>(dbContext, unitOfWork)
     where T : class
     where TCreateRequest : CreateCommand<T>
{ }

public class CreateCommandHandler<T>(DbContext dbContext, EFUnitOfWork<DbContext> unitOfWork) 
    : CreateCommandHandler<T, CreateCommand<T>, DbContext, EFUnitOfWork<DbContext>>(dbContext, unitOfWork), ICreateCommandHandler<T>
    where T : class
{ }