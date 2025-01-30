using Alchemist.Common;
using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Product.Import.Background;

internal class ImportProduct : IImportProduct
{
    public string Name { get; set; }
    public string ShopName { get; set; }
    public string Url { get; set; }
    public bool Success { get; set; }

    public ItemProcessStatus Status { get; set; }
}
