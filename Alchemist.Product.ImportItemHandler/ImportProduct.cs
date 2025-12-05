using Import.Interfaces;
using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Product.ImportItemHandler;

internal class ImportProduct : IImportProductItem
{
    public string Name { get; set; }
    public string ShopName { get; set; }
    public string ShopUrl { get; set; }
    public string Url { get; set; }
    public bool Success { get; set; }

    public ResultStatus Status { get; set; }
}