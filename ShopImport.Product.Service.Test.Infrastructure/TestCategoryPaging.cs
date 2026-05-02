using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;

namespace ShopImport.Product.Service.Test.Infrastructure;

public class TestCategoryPaging<TCategory> : CategoryPaging<TCategory>
     where TCategory : class, ICategoryProducts, new()
{
    protected override string PreparePath(string path) => path;
}