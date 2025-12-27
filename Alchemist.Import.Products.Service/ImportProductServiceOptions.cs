using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Import.Products.Service;

public class ImportProductServiceOptions
{
    public int? PageProductCount { get; set; }

    public object? ProductLoadData { get; set; }

    public object? CategoryLoadData { get; set; }

    public UrlFormatType ProductUrlFormatType { get; set; }

    public UrlFormatType CategoryUrlFormatType { get; set; }
}