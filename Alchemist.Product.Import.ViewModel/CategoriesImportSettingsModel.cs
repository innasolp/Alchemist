using Alchemist.Product.Import.Model.Infrastructure;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.Model;

public class CategoriesImportSettingsModel : SettingsModelBase
{
    [JsonIgnore]
    public override TabType Tab => TabType.Categories;
}
