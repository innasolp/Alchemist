using Shop = Alchemist.Product.Entities.Shop;

namespace Alchemist.Product.ShopWebApp.IntegratonTest.Infrastructure;

internal class TestRepository
{
    public static List<Shop> GetShopsTestData(int count)
    {
        var shops = new List<Shop>();
        for (int i = 0; i < count; i++)
        {
            var shop = new Shop { Name = $"TestShop{i + 1}", Url = $"https://testshop{i + 1}" };
            shops.Add(shop);
        }

        return shops;
    }
}
