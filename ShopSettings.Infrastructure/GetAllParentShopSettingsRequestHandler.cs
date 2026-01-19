using MediatR;
using ShopSettings.UnitOfWork;

namespace ShopSettings.Infrastructure;

public class GetAllParentShopSettingsRequestHandler(IShopSettingsRepository repository)
    : IRequestHandler<GetAllParentShopSettingsRequest, List<Alchemist.Product.Data.ShopSettings>>
{
    private readonly IShopSettingsRepository _repository = repository;

    public Task<List<Alchemist.Product.Data.ShopSettings>> Handle(GetAllParentShopSettingsRequest request, CancellationToken cancellationToken = default)
    {
        return _repository.GetAllParentShopSettings(cancellationToken);
    }
}