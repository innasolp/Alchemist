using Mediator.Infrastructure.Command;
using Microsoft.EntityFrameworkCore.Storage;
using UnitOfWork;

namespace Mediator.Infrastructure;

public class EFUpdateCommandHandler<TCommand, T>(IRepository<T> repository, IUnitOfWork<IDbContextTransaction> unitOfWork)
    : UpdateCommandHandler<T, TCommand, IRepository<T>, IDbContextTransaction, IUnitOfWork<IDbContextTransaction>>(repository, unitOfWork)    
    where T : class
    where TCommand : UpdateCommand<T>
{
}