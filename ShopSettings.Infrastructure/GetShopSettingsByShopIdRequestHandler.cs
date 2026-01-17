using MediatR;
using ShopSettings.UnitOfWork;
namespace ShopSettings.Infrastructure;

public class GetShopSettingsByShopIdRequestHandler(IShopSettingsRepository repository) 
    : IRequestHandler<GetShopSettingsByShopIdRequest, Alchemist.Product.Data.ShopSettings>
{
    private readonly IShopSettingsRepository _repository = repository;

    public Task<Alchemist.Product.Data.ShopSettings> Handle(GetShopSettingsByShopIdRequest request, CancellationToken cancellationToken = default)
    {
        return _repository.GetShopSettingsByShopId(request.ShopId, request.SettingType, cancellationToken)!;
    }
}