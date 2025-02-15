using Alchemist.Product.Import.WebApp.Infrastructure;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.WebApp.Models;

public class CategoriesImportSettingsModel : SettingsModelBase
{
    [JsonIgnore]
    public override SettingsType Type => SettingsType.Categories;
}
