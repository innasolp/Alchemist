using Alchemist.Product.Model;

namespace Alchemist.Product.ShopWebApp.Models;

internal class ShopJsonModelBinder(ILogger<ShopJsonModelBinder> logger) : ModelJsonBinder(logger)
{
    protected override Type ModelType => typeof(ShopModel);
}
