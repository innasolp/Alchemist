using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace Alchemist.Product.Import.WebApp.Models;

public abstract class SettingsModelBase(int shopId, Guid shopGuid) : ISettingsModel
{
    public Guid Guid { get; } = Guid.NewGuid();

    public abstract TabType Tab { get; }

    public Guid ShopGuid { get; } = shopGuid;

    public int ShopId { get; } = shopId;

    [Required]
    public string Name { get; set; }

    public virtual void Update(SettingsModelBase source)
    {
        Name = source.Name;
    }

    public override string ToString()
    {
        return $"{nameof(Name)}:{Name};{nameof(Guid)}:{Guid};{nameof(ShopGuid)}:{ShopGuid};{nameof(Tab)}:{Tab}";
    }
}
