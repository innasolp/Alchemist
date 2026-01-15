using Alchemist.Product.Data;
using UnitOfWork;

namespace Alchemist.Product.UnitOfWork;

public class ProductUnitOfWork(AlchemyContext context) : EFUnitOfWork<AlchemyContext>(context)
{
}
