using Mediator.Infrastructure;
using Mediator.Infrastructure.Command;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using Shop.UnitOfWork;
using UnitOfWork;

namespace Shop.Infrastructure;

public sealed class CreateShopCommandHandler(IShopRepository repository,
    IUnitOfWork<IDbContextTransaction> unitOfWork,
    IPublisher eventPublisher) 
    : EFCreateCommandHandler<Alchemist.Product.Data.Shop>(repository, unitOfWork)
{
    private readonly IPublisher _eventPublisher = eventPublisher;

    protected override async Task<Alchemist.Product.Data.Shop> HandlerRequest(CreateCommand<Alchemist.Product.Data.Shop> request,
        CancellationToken cancellationToken = default)
    {
        var newShop = await base.HandlerRequest(request, cancellationToken);        
        await _eventPublisher.Publish(new CreateShopEvent(newShop, DateTime.Now), cancellationToken);
        return newShop;
    }
}