namespace Alchemist.Test.ShopApiFactory;

public static class ShopTestRepository
{
    private static readonly List<Product.Data.Shop> _shops = [];

    public static List<Product.Data.Shop> CreateShopsTestData(int count)
    {
        var shops = new List<Product.Data.Shop>();
        for (int i = 0; i < count; i++)
        {
            var shop = new Product.Data.Shop { Name = $"TestShop{i + 1}", Url = $"https://testshop{i + 1}" };
            shops.Add(shop);
        }

        return shops;
    }
}