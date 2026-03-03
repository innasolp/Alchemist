using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;

namespace ShopImport.Product.Service.Stateful;

internal class ServiceState<TCategory>
    where TCategory : class, ICategoryProducts
{
    public CategoryProcessState CategoryProcessState { get; private set; } = CategoryProcessState.Start();

    public TCategory? Category { get; set;  }

    public ProductShopCategory? ProductShopCategory { get; private set; }

    public string? CurrentCategoryProductItemId { get; set; }

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