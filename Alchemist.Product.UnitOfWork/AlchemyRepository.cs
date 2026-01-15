using Alchemist.Product.Data;
using UnitOfWork;

namespace Alchemist.Product.UnitOfWork;

public class AlchemyRepository<T>(AlchemyContext alchemyContext) : EFRepository<T, AlchemyContext>(alchemyContext), IRepository<T>
    where T:class
{
}