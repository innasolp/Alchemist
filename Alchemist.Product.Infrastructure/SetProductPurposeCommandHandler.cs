using Alchemist.Product.Data;
using Alchemist.Product.Infrastructure.Interfaces;
using Mediator.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using UnitOfWork;

namespace Alchemist.Product.Infrastructure;

public class SetProductPurposeCommandHandler(IProductPurposeRepository productPurposeRepository, IUnitOfWork<IDbContextTransaction> productUnitOfWork)
    : TransactionCommandHandler<ProductPurpose, SetProductPurposeCommand, IDbContextTransaction, IUnitOfWork<IDbContextTransaction>>(productUnitOfWork)
{
    private readonly IProductPurposeRepository _productPurposeRepository = productPurposeRepository;

    protected override Task<ProductPurpose> HandleCommand(SetProductPurposeCommand request, CancellationToken cancellationToken = default)
    {
        return _productPurposeRepository.SetProductPurpose(request.Entity, cancellationToken);
    }
}