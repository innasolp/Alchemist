using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using System.Web;

namespace Alchemist.Import.Factory.Product.Json.Infrastructure;

public class CategoryUrlPaging <TCategory> : CategoryPaging<TCategory> 
    where TCategory : class, ICategoryProducts
{
    protected override string PreparePath(string path) => HttpUtility.UrlEncode(path);
}