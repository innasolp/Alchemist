using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Model.Infrastructure;

public static class EntityExtensions
{
    public static ShopModel ToModel(this IShop shop)
    {
        return new ShopModel { Id = shop.Id, Name = shop.Name };
    }

    public static void Update(this ShopModel shopModel, IShop shop)
    {
        shopModel.Name = shop.Name;
    }
}
