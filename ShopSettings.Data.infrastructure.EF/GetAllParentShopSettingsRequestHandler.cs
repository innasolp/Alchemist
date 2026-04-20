using Alchemist.Product.Data;
using Db.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ShopSettings.Data.Infrastructure;

namespace ShopSettings.Data.infrastructure.EF;

public class GetAllParentShopSettingsRequestHandler(AlchemyContext context) : IRequestHandler<GetAllParentShopSettingsRequest, List<Alchemist.Product.Data.ShopSettings>>
{
    private readonly AlchemyContext _context = context;

    public Task<List<Alchemist.Product.Data.ShopSettings>> Handle(GetAllParentShopSettingsRequest request, CancellationToken cancellationToken)
    {
        return _context.ShopSettings.Where(s => s.ParentSettingsId == null).ToListAsync(cancellationToken);
    }
}