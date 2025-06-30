using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.WebApp.Models;

public class CategoriesImportSettingsModel(int shopId, Guid shopGuid) : SettingsModelBase(shopId, shopGuid), ICategoryImportSettingsModel
{
    [JsonIgnore]
    public override TabType Tab => TabType.Categories;
}
