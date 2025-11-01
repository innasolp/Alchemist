using Shop = Alchemist.Product.Entities.Shop;

namespace Alchemist.Test.ShopApiFactory;

public static class ShopTestRepository
{
    private static readonly List<Shop> _shops = [];

    public static List<Shop> CreateShopsTestData(int count)
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
