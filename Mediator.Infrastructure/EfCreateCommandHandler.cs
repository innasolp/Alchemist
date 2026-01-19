using Mediator.Infrastructure.Command;
using Microsoft.EntityFrameworkCore.Storage;
using UnitOfWork;

namespace Mediator.Infrastructure;

public class EFCreateCommandHandler<TRequest, T>(IRepository<T> repository, IUnitOfWork<IDbContextTransaction> unitOfWork)
    : CreateCommandHandler<T, TRequest, IRepository<T>, IDbContextTransaction, IUnitOfWork<IDbContextTransaction>>(repository, unitOfWork)
    where T : class
    where TRequest: CreateCommand<T>
{
}

public class EFCreateCommandHandler<T>(IRepository<T> repository, IUnitOfWork<IDbContextTransaction> unitOfWork)
    : CreateCommandHandler<T, CreateCommand<T>, IRepository<T>, IDbContextTransaction, IUnitOfWork<IDbContextTransaction>>(repository, unitOfWork)
    where T : class
{
}