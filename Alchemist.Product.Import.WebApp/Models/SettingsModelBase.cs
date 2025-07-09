using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.WebApp.Models;

[method: JsonConstructor]
public abstract class SettingsModelBase(int shopId, Guid shopGuid) : ISettingsModel
{
    [JsonInclude]
    public Guid Guid { get; private set; } = Guid.NewGuid();

    public abstract TabType Tab { get; }

    [JsonInclude]
    public Guid ShopGuid { get; private set; } = shopGuid;

    [JsonInclude]
    public int ShopId { get; set; } = shopId;

    [Required]
    [JsonInclude]
    public string Name { get; set; }

    public override string ToString()
    {
        return $"{nameof(Name)}:{Name};{nameof(Guid)}:{Guid};{nameof(ShopGuid)}:{ShopGuid};{nameof(Tab)}:{Tab}";
    }
}
