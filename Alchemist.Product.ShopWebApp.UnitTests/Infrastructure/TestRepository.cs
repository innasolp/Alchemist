namespace Alchemist.Product.ShopWebApp.UnitTests.Infrastructure;

internal static class TestRepository
{
    public static List<Entities.Shop> GetShopsTestData(int count)
    {
        var shops = new List<Entities.Shop>();
        for (int i = 0; i < count; i++)
        {
            var shop = new Entities.Shop { Name = $"TestShop{i + 1}", Url = $"https://testshop{i + 1}" };
            shops.Add(shop);
        }

        return shops;
    }
}
