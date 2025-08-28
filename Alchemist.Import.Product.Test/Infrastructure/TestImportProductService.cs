using Alchemist.Import.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using Alchemist.Test.Import.Service.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Product.Test.Infrastructure;

public class TestImportProductService<TCategory, TProductItem>(ILogger logger,
    IProductShopModel shopUrlModel, 
    ILoaderService loader,
    IProductItemHandler itemHandler) 
    : ShopImportCategoryProductsService<TCategory, TProductItem>(logger, shopUrlModel, loader, itemHandler), ITestService
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
{
    private string _name;

    public override string Name => _name;
    
    public void SetName(string name)
    {
        _name = name;
    }

    private int _pageCount = 1000;

    protected override int PageProductCount => _pageCount;

    public void SetPageProductCount(int pageCount)
    {
        _pageCount = pageCount;
    }

    protected override string GetApiUrl(ICategoryProductItem productItem)
    {
        return string.Format(ProductShopModel.ProductUrl, productItem.Name);
    }

    public string GetTestApiUrl(ICategoryProductItem productItem)
    {
        return GetApiUrl(productItem);
    }

    protected override bool IsEndOfCategory(TCategory category)
    {
        return !(category.CategoryProductItems?.Length > 0);
    }
}
