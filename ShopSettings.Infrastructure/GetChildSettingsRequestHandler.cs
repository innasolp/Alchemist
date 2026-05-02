using MediatR;
using ShopSettings.UnitOfWork;

namespace ShopSettings.Infrastructure;

public class GetChildSettingsRequestHandler(IShopSettingsRepository repository)
    : IRequestHandler<GetChildSettingsRequest, List<Alchemist.Product.Data.ShopSettings>>
{
    private readonly IShopSettingsRepository _repository = repository;

    public Task<List<Alchemist.Product.Data.ShopSettings>> Handle(GetChildSettingsRequest request, CancellationToken cancellationToken = default)
    {
        return _repository.GetChildSettings(request.ParentSettingsId, cancellationToken);
    }
}