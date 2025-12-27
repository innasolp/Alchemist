using Alchemist.Import.Products.Interfaces;


namespace Alchemist.Import.ProductService.Test.Infrastructure;

public class TestCategory : ICategoryProducts
{
    public TestCategoryProduct[] CategoryProductItems { get; set; }

    public int? TotalCount { get; set; }

    ICategoryProductItem[] ICategoryProducts.CategoryProductItems => CategoryProductItems;
}

public class TestCategoryProduct : ICategoryProductItem
{
    public string Id { get; set; }

    public string ItemUrl { get; set; }

    public string Currency { get; set; }

    public double Price { get; set; }

    public string Name { get; set; }

    public int CategoryItemId { get; set; }
    public string Brand { get ; set; }
}

public class TestProductItem : IProductItem
{
    public string ItemId { get; set; }

    public string Name { get; set; }

    public string[]? Components { get; set; }

    public string Brand { get; set; }

    public string Country { get; set; }

    public string Comment { get; set; }

    public string ProductType { get; set; }

    public string[] Purposes { get; set; }

    public string Articul { get; set; }

    public string Currency { get; set; }
    public double Price { get; set; }
    public string Url { get; set; }
    public string ApiUrl { get; set; }
    public int CategoryId { get; set; }
}
