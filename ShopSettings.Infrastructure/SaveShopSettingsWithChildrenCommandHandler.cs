using Mediator.Infrastructure;
using Mediator.Infrastructure.Events;
using MediatR;
using ShopSettings.UnitOfWork;
using UnitOfWork;

namespace ShopSettings.Infrastructure;

public sealed class SaveShopSettingsWithChildrenCommandHandler<TTransaction>(IShopSettingsRepository repository,
    IUnitOfWork<TTransaction> unitOfWork,
    IPublisher eventPublisher)
    : TransactionCommandHandler<(Alchemist.Product.Data.ShopSettings, IEnumerable<Alchemist.Product.Data.ShopSettings>), SaveShopSettingsWithChildrenCommand, TTransaction, IUnitOfWork<TTransaction>>(unitOfWork)
{
    private readonly IShopSettingsRepository _repository = repository;

    private readonly IPublisher _eventPublisher = eventPublisher;

    protected override async Task<(Alchemist.Product.Data.ShopSettings, IEnumerable<Alchemist.Product.Data.ShopSettings>)>
        HandleCommand(SaveShopSettingsWithChildrenCommand request, CancellationToken cancellationToken)
    {
        var shopSettingsId = request.ParentShopSettings.Id;

        var (shopSettings, services) = await _repository.SaveShopSettingsWithChildren(request.ParentShopSettings, request.ChildrenSettings, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        if (shopSettingsId == 0)
            await _eventPublisher.Publish(new CreationEvent<Alchemist.Product.Data.ShopSettings>(Messages.ShopSettingsCreated, shopSettings, DateTime.Now), cancellationToken);

        return (shopSettings, services);
    }
}