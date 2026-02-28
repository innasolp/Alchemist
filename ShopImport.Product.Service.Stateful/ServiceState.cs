using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;

namespace ShopImport.Product.Service.Stateful;

internal class ServiceState<TCategory>
    where TCategory : class, ICategoryProducts
{
    public CategoryState CategoryState { get; private set; } = CategoryState.Start();

    public TCategory? Category { get; set;  }

    public IProductShopCategory? ProductShopCategory { get; private set; }

    public ICategoryProductItem? CurrentCategoryProductItem { get; set; }

    public List<ICategoryProductItem> HandledCategoryProductItems { get; } = [];

    public void Reset()
    {
        CategoryState.Reset();
        Category = null;
        ProductShopCategory = null;
        CurrentCategoryProductItem = null;
        HandledCategoryProductItems.Clear();
    }

    public void Start(IProductShopCategory productShopCategory)
    {
        ProductShopCategory = productShopCategory;
        CategoryState = CategoryState.Start(ProductShopCategory.Path);
    }
}