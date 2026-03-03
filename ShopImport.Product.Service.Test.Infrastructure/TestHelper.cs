using Alchemist.Import.Products.Interfaces;
using Moq;

namespace ShopImport.Product.Service.Test.Infrastructure;

public static class TestHelper
{
    public static Mock<IProductShopCategory> CreateProductShopCategoryMock()
    {
        var categoryMock = new Mock<IProductShopCategory>();
        categoryMock.Setup(c => c.ItemId).Returns(new Random().Next(10000));
        categoryMock.Setup(c => c.Category).Returns(Guid.NewGuid().ToString());
        categoryMock.Setup(c => c.Path).Returns(Guid.NewGuid().ToString());
        return categoryMock;
    }

    public static TestCategory CreateCategoryWithProducts(int productCount, int totalCount, int page)
    {        
        var productsCategory = new TestCategory
        {
            CategoryProductItems = new TestCategoryProduct[productCount],
            TotalCount = totalCount,
            Page = page
        };        

        for (var i = 0; i < productsCategory.CategoryProductItems.Length; i++)
        {
            productsCategory.CategoryProductItems[i] = new TestCategoryProduct()
            {
                Name = Guid.NewGuid().ToString(),
                ItemPath = Guid.NewGuid().ToString(),
                Id = Guid.NewGuid().ToString()
            };
        }
        return productsCategory;
    }

    public static TestProductItem CreateProductItem(ICategoryProductItem categoryProduct)
    {
        return new TestProductItem()
        {
            ItemId = categoryProduct.Id,
            Articul = Guid.NewGuid().ToString(),
            Brand = Guid.NewGuid().ToString(),
            Name = categoryProduct.Name
        };
    }
}