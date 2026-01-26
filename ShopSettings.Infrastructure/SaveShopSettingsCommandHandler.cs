using Mediator.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using ShopSettings.UnitOfWork;
using UnitOfWork;

namespace ShopSettings.Infrastructure;

public sealed class SaveShopSettingsCommandHandler(IShopSettingsRepository repository, 
    IUnitOfWork<IDbContextTransaction> unitOfWork,
    IPublisher eventPublisher)
    : TransactionCommandHandler<Alchemist.Product.Data.ShopSettings, SaveShopSettingsCommand, IDbContextTransaction, IUnitOfWork<IDbContextTransaction>>(unitOfWork),
    IRequestHandler<SaveShopSettingsCommand, Alchemist.Product.Data.ShopSettings>    
{
    private readonly IShopSettingsRepository _repository = repository;

    private readonly IPublisher _eventPublisher = eventPublisher;

    protected override async Task<Alchemist.Product.Data.ShopSettings> HandlerRequest(SaveShopSettingsCommand request, CancellationToken cancellationToken = default)
    {
        var shopSettingsId = request.ShopSettings.Id;

        var saved = await _repository.SaveShopSettings(request.ShopSettings, cancellationToken)!;
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        if(shopSettingsId == 0)
            await _eventPublisher.Publish(new CreateShopSettingsEvent(saved, DateTime.Now), cancellationToken);

        return saved;
    }
}