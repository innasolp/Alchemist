using Alchemist.Product.Data;
using Db.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ShopSettings.Data.Infrastructure;

namespace ShopSettings.Data.infrastructure.EF;

public class GetChildSettingsRequestHandler(AlchemyContext context) : IRequestHandler<GetChildSettingsRequest, List<Alchemist.Product.Data.ShopSettings>>
{
    private readonly AlchemyContext _context = context;

    public Task<List<Alchemist.Product.Data.ShopSettings>> Handle(GetChildSettingsRequest request, CancellationToken cancellationToken)
    {
        return _context.ShopSettings.Where(s => s.ParentSettingsId == request.ParentSettingsId).ToListAsync(cancellationToken);
    }
}