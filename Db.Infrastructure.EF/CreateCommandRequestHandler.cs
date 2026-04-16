using Db.Infrastructure.Commands;
using Microsoft.EntityFrameworkCore;

namespace Db.Infrastructure.EF;

public class CreateCommandHandler<T, TCreateCommand>(IUnitOfWork unitOfWork, DbContext dbContext)
    : TransactionalCommandHandler<TCreateCommand>(unitOfWork)
    where TCreateCommand : CreateCommand<T>
    where T:class
{    

    protected override async Task HandleCommand(TCreateCommand command, CancellationToken cancellationToken)
    {
        await dbContext.Set<T>().AddAsync(command.Entity, cancellationToken);
    }
}