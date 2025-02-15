using Alchemist.Product.Import.WebApp.Infrastructure;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.WebApp.Models;

public class ProductsImportSettingsModel:SettingsModelBase
{
    [JsonIgnore]
    public override SettingsType Type => SettingsType.Products;
}
