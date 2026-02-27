using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using System.Web;

namespace Alchemist.Import.Factory.Product.Json;

internal class CategoryItemUrlPaging<TCategory> : CategoryItemPaging<TCategory>
    where TCategory : class, ICategoryProducts, IPagingItem
{
    protected override string PreparePath(string path) => HttpUtility.UrlEncode(path);
}