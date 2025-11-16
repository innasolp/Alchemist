using Alchemist.Web.ModelBinder;

namespace Alchemist.Product.ShopWebApp.Models;

internal class ShopModelJsonBinder(ILogger<ShopModelJsonBinder> logger) : ModelJsonBinder(logger)
{
    protected override Type ModelType => typeof(ShopModel);
}
