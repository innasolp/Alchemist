using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using System.Web;

namespace Alchemist.Import.Factory.Product.Json;

internal class CategoryUrlPaging <TCategory> : CategoryPaging<TCategory> 
    where TCategory : class, ICategoryProducts
{
    protected override string PreparePath(string path) => HttpUtility.UrlEncode(path);
}