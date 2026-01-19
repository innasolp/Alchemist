using Mediator.Infrastructure;
using Mediator.Infrastructure.Command;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using UnitOfWork;

namespace Shop.Infrastructure;

public sealed class CreateShopCategoryCommandHandler(IRepository<Alchemist.Product.Data.ShopCategory> repository,
    IUnitOfWork<IDbContextTransaction> unitOfWork,
    IPublisher eventPublisher)
    : EFCreateCommandHandler<Alchemist.Product.Data.ShopCategory>(repository, unitOfWork)
{
    private readonly IPublisher _eventPublisher = eventPublisher;

    protected override async Task<Alchemist.Product.Data.ShopCategory> HandlerRequest(CreateCommand<Alchemist.Product.Data.ShopCategory> request,
        CancellationToken cancellationToken = default)
    {
        var newShop = await base.HandlerRequest(request, cancellationToken);
        await _eventPublisher.Publish(new CreateShopCategoryEvent(newShop, DateTime.Now), cancellationToken);
        return newShop;
    }
}