using Mediator.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using ShopSettings.UnitOfWork;
using UnitOfWork;

namespace ShopSettings.Infrastructure;

public class SaveShopSettingsWithChildrenCommandHandler(IShopSettingsRepository repository, IUnitOfWork<IDbContextTransaction> unitOfWork)
    : CommandHandler<List<Alchemist.Product.Data.ShopSettings>, SaveShopSettingsWithChildrenCommand, IDbContextTransaction, IUnitOfWork<IDbContextTransaction>>(unitOfWork)
{
    private readonly IShopSettingsRepository _repository = repository;

    protected override Task<List<Alchemist.Product.Data.ShopSettings>> 
        HandlerRequest(SaveShopSettingsWithChildrenCommand request, CancellationToken cancellationToken)
    {
        return _repository.SaveShopSettingsWithChildren(request.ParentShopSettings, request.ChildrenSettings, cancellationToken);
    }
}