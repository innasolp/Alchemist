using Alchemist.Product.Model;

namespace Alchemist.Product.ShopWebApp.Models;

internal class ShopModelJsonBinder(ILogger<ShopModelJsonBinder> logger) : ModelJsonBinder(logger)
{
    protected override Type ModelType => typeof(ShopModel);
}
