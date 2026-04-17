using Db.Infrastructure.Commands;
using Microsoft.EntityFrameworkCore;

namespace Db.Infrastructure.EF;

public class UpdateCommandHandler<T, TUpdateCommand>(IUnitOfWork unitOfWork, DbContext dbContext)
    : TransactionalCommandHandler<TUpdateCommand>(unitOfWork)
    where T:class
    where TUpdateCommand : UpdateCommand<T>
{    
    protected override Task HandleCommand(TUpdateCommand command, CancellationToken cancellationToken)
    {
        dbContext.Set<T>().Update(command.Entity);
        return Task.CompletedTask;
    }
}

public class UpdateCommandHandler<T>(IUnitOfWork unitOfWork, DbContext dbContext)
    : UpdateCommandHandler<T, UpdateCommand<T>>(unitOfWork, dbContext)
    where T : class
{
}