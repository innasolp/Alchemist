using Alchemist.Import.Products.Service;
using Json.CustomSerialization;

namespace Alchemist.Import.Factory.Product.Json;

internal class ImportProductJsonServiceOptions : ImportProductServiceOptions
{
    public IJsonSettings CategoryJsonSettings { get; set; }
}