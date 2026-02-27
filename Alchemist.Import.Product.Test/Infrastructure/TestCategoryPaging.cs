using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;

namespace Alchemist.Import.ProductService.Test.Infrastructure;

internal class TestCategoryPaging<TCategory> : CategoryPaging<TCategory>
     where TCategory : class, ICategoryProducts, new()
{
    protected override string PreparePath(string path) => path;
}