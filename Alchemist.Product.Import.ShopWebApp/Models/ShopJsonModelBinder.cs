using Alchemist.Web.ModelBinder;

namespace Alchemist.Product.ShopWebApp.Models;

internal class ShopJsonModelBinder(ILogger<ShopJsonModelBinder> logger) : ModelJsonBinder(logger)
{
    protected override Type ModelType => typeof(ShopModel);
}
