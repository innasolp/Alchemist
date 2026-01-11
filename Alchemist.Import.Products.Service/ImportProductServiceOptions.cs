using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Import.Products.Service;

public class ImportProductServiceOptions
{
    public int? PageProductCount { get; set; }

    public object? ProductLoadData { get; set; }

    public object? CategoryLoadData { get; set; }

    public PathFormatType ProductPathFormatType { get; set; }

    public PathFormatType CategoryPathFormatType { get; set; }
}