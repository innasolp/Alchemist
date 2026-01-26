using Alchemist.Product.Data;
using Alchemist.Product.UnitOfWork.Interfaces;
using Mediator.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using UnitOfWork;

namespace Alchemist.Product.Infrastructure;

public class SetProductComponentCommandHandler(IProductComponentRepository repository, IUnitOfWork<IDbContextTransaction> productUnitOfWork) 
    : TransactionCommandHandler<ProductComponent, SetProductComponentCommand, IDbContextTransaction, IUnitOfWork<IDbContextTransaction>>(productUnitOfWork)
{
    private readonly IProductComponentRepository _repository = repository;

    protected override Task<ProductComponent> HandlerRequest(SetProductComponentCommand request, CancellationToken cancellationToken = default)
    {
        return _repository.SetProductComponent(request.Entity, cancellationToken);
    }
}