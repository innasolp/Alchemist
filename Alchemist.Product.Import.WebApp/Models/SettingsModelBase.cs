using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace Alchemist.Product.Import.WebApp.Models;

public abstract class SettingsModelBase(int shopId, Guid shopGuid) : ISettingsModel
{
    public Guid Guid { get; set; } = Guid.NewGuid();

    public abstract TabType Tab { get; }

    public Guid ShopGuid { get; set; } = shopGuid;

    public int ShopId { get; set; } = shopId;

    [Required]
    public string Name { get; set; }

    public override string ToString()
    {
        return $"{nameof(Name)}:{Name};{nameof(Guid)}:{Guid};{nameof(ShopGuid)}:{ShopGuid};{nameof(Tab)}:{Tab}";
    }
}
