using Alchemist.Product.Data;
using Db.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ShopSettings.Data.Infrastructure;

namespace ShopSettings.Data.infrastructure.EF;

public class GetShopSettingsByShopIdRequestHandler(AlchemyContext context) : IRequestHandler<GetShopSettingsByShopIdRequest, Alchemist.Product.Data.ShopSettings?>
{
    private readonly AlchemyContext _context = context;

    public Task<Alchemist.Product.Data.ShopSettings?> Handle(GetShopSettingsByShopIdRequest request, CancellationToken cancellationToken)
    {
        return _context.ShopSettings.FirstOrDefaultAsync(s => s.ShopId == request.ShopId && s.Type == request.SettingType && s.IsActual != false, cancellationToken);
    }
}
