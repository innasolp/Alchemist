using Alchemist.Product.Data;
using UnitOfWork;

namespace Shop.UnitOfWork;

public class ShopUnitOfWork(AlchemyContext context) : EFUnitOfWork<AlchemyContext>(context)
{
}