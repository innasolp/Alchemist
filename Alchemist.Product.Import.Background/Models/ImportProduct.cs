using Alchemist.Common;
using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Product.Import.Background.Models;

internal class ImportProduct : IImportProduct
{
    public string Name { get; set; }
    public string ShopName { get; set; }
    public string Url { get; set; }
    public bool Success { get; set; }

    public ResultStatus Status { get; set; }
}
