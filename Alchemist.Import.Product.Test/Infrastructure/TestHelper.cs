using Alchemist.Import.Products.Interfaces;
using Moq;

namespace Alchemist.Import.Product.Test.Infrastructure;

internal static class TestHelper
{
    public static Mock<IProductShopCategoryModel> CreateCategoryMock()
    {
        var categoryMock = new Mock<IProductShopCategoryModel>();
        categoryMock.Setup(c => c.ItemId).Returns(new Random().Next(10000));
        categoryMock.Setup(c => c.Category).Returns(Guid.NewGuid().ToString());
        return categoryMock;
    }

    public static TestCategory CreateCategoryWithProducts()
    {
        var productsCategory = new TestCategory();
        productsCategory.CategoryProductItems = new TestCategoryProduct[new Random().Next(10)];
        for (var i = 0; i < productsCategory.CategoryProductItems.Length; i++)
        {
            productsCategory.CategoryProductItems[i] = new TestCategoryProduct()
            {
                Name = Guid.NewGuid().ToString(),
                ItemUrl = Guid.NewGuid().ToString(),
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
