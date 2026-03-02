using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;

namespace ShopImport.Product.Service.Stateful;

internal class ServiceState<TCategory>
    where TCategory : class, ICategoryProducts
{
    public CategoryState CategoryState { get; private set; } = CategoryState.Start();

    public TCategory? Category { get; set;  }

    public string? ProductShopCategoryPath { get; private set; }

    public string? CurrentCategoryProductItemId { get; set; }

    public List<string> HandledCategoryProductItemIds { get; } = [];

    public void Reset()
    {
        CategoryState.Reset();
        Category = null;
        ProductShopCategoryPath = null;
        CurrentCategoryProductItemId = null;
        HandledCategoryProductItemIds.Clear();
    }

    public void Start(IProductShopCategory productShopCategory)
    {
        ProductShopCategoryPath = productShopCategory.Path;
        CategoryState = CategoryState.Start(ProductShopCategoryPath);
    }
}