using Alchemist.Product.Data;
using UnitOfWork;

namespace ShopSettings.UnitOfWork;

public class ShopSettingsUnitOfWork(AlchemyContext context) : EFUnitOfWork<AlchemyContext>(context)
{
}
