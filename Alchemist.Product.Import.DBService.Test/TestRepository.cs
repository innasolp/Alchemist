namespace Alchemist.Product.Import.DBService.Test;

public static class TestRepository
{
    public static List<Data.Shop> GetShopsTestData(int count)
    {
        var shops = new List<Data.Shop>();
        for (int i=0;i< count; i++)
        {
            var shop = new Data.Shop { Name = $"TestShop{i + 1}", Url = $"https://testshop{i + 1}" };
            shops.Add(shop);
        }

        return shops;
    }
}
