using Mediator.Infrastructure;
using Mediator.Infrastructure.Events;
using MediatR;
using ShopSettings.UnitOfWork;
using UnitOfWork;

namespace ShopSettings.Infrastructure;

public sealed class SaveShopSettingsCommandHandler<TTransaction>(IShopSettingsRepository repository, 
    IUnitOfWork<TTransaction> unitOfWork,
    IPublisher eventPublisher)
    : TransactionCommandHandler<Alchemist.Product.Data.ShopSettings, SaveShopSettingsCommand, TTransaction, IUnitOfWork<TTransaction>>(unitOfWork),
    IRequestHandler<SaveShopSettingsCommand, Alchemist.Product.Data.ShopSettings>    
{
    private readonly IShopSettingsRepository _repository = repository;

    private readonly IPublisher _eventPublisher = eventPublisher;

    protected override async Task<Alchemist.Product.Data.ShopSettings> HandleCommand(SaveShopSettingsCommand request, CancellationToken cancellationToken = default)
    {
        var shopSettingsId = request.ShopSettings.Id;

        var saved = await _repository.SaveShopSettings(request.ShopSettings, cancellationToken)!;
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        if(shopSettingsId == 0)
            await _eventPublisher.Publish(new CreationEvent<Alchemist.Product.Data.ShopSettings>(Messages.ShopSettingsCreated, saved, DateTime.Now),
                cancellationToken);

        return saved;
    }
}