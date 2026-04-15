using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using System.Text.Json.Serialization;

namespace ShopImport.Product.Service.Stateful;

internal class ServiceState<TCategory>
    where TCategory : class, ICategoryProducts, new()
{
    [JsonInclude]
    public CategoryProcessState CategoryProcessState { get; private set; } = CategoryProcessState.Start();

    [JsonInclude]
    public TCategory? Category { get; set;  }

    [JsonInclude]
    public ProductShopCategory? ProductShopCategory { get; private set; }

    [JsonInclude]
    public string? CurrentCategoryProductItemId { get; set; }

    [JsonInclude]
    public List<string> HandledCategoryProductItemIds { get; } = [];

    public void Reset()
    {
        CategoryProcessState.Reset();
        Category = null;
        ProductShopCategory = null;
        CurrentCategoryProductItemId = null;
        HandledCategoryProductItemIds.Clear();
    }

    public void Start(IProductShopCategory productShopCategory)
    {
        ProductShopCategory = new ProductShopCategory
        {
            Path = productShopCategory.Path,
            Category = productShopCategory.Category,
            ItemId = productShopCategory.ItemId
        };

        CategoryProcessState = CategoryProcessState.Start(ProductShopCategory.Path);
    }
}