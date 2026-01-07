using Alchemist.Import.Products.Service;
using Json.CustomSerialization;

namespace Alchemist.Import.Product.Json.Service;

public class ImportProductJsonServiceOptions : ImportProductServiceOptions
{
    public IJsonSettings CategoryJsonSettings { get; set; }
}