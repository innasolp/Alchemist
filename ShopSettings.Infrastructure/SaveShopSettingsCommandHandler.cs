using Mediator.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using ShopSettings.UnitOfWork;
using UnitOfWork;

namespace ShopSettings.Infrastructure;

public class SaveShopSettingsCommandHandler(IShopSettingsRepository repository, IUnitOfWork<IDbContextTransaction> unitOfWork)
    : CommandHandler<Alchemist.Product.Data.ShopSettings, SaveShopSettingsCommand, IDbContextTransaction, IUnitOfWork<IDbContextTransaction>>(unitOfWork),
    IRequestHandler<SaveShopSettingsCommand, Alchemist.Product.Data.ShopSettings>    
{
    private readonly IShopSettingsRepository _repository = repository;

    protected override Task<Alchemist.Product.Data.ShopSettings> HandlerRequest(SaveShopSettingsCommand request, CancellationToken cancellationToken = default)
    {
        return _repository.SaveShopSettings(request.ShopSettings, cancellationToken)!;
    }
}