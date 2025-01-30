using System.Text.Json;

namespace Alchemist.Product.Shop.GoldApple.Model.Tests
{
    public class CategoryTests
    {
        private readonly CategoryProducts? _category;
        public CategoryTests() 
        {
            using var stream = File.OpenRead("GoldAppleCategory.json");
            _category = JsonSerializer.Deserialize<CategoryProducts>(stream);
            stream.Close();
        }

        [Fact]
        public void CategoryJsonDeserializationSuccess()
        {
            Assert.NotNull(_category);
            Assert.NotNull(_category.Data);
            Assert.NotNull(_category.Data.Products);
        }

        [Fact]
        public void CategoryProductPricesNotNull()
        {
            Assert.True(_category?.Data.Products.All(p => p.ProductItemPrice != null));
            Assert.True(_category?.Data.Products.All(p => p.Price > 0));
        }
    }
}