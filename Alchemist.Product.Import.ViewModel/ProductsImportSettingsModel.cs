using Alchemist.Product.Import.Model.Infrastructure;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.Model;

public class ProductsImportSettingsModel : SettingsModelBase
{
    [JsonIgnore]
    public override TabType Tab => TabType.Products;
}
