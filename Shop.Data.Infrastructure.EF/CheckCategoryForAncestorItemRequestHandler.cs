using Alchemist.Product.Data;
using Db.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Shop.Data.Infrastructure.EF;

public class CheckCategoryForAncestorItemRequestHandler(AlchemyContext context) : IRequestHandler<CheckCategoryForAncestorItemRequest, bool?>
{
    private AlchemyContext Context { get; } = context;

    public Task<bool?> Handle(CheckCategoryForAncestorItemRequest request, CancellationToken cancellationToken)
    {
        return Context.ShopCategories
            .Where(e => e.Id == request.Id)
            .Select(e => (bool?)Context.ShopCategories
                .Where(ancestor => ancestor.ItemId == request.AncestorItemId)
                .Any(ancestor => e.Path.Contains("/" + ancestor.Id + "/"))
            )
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
}