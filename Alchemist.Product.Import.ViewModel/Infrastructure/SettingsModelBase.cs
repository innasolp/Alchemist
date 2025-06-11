using System.ComponentModel;

namespace Alchemist.Product.Import.Model.Infrastructure;

public enum TabType
{
    [Description("Products")]
    [Category("ImportProducts")]
    Products = 1,

    [Description("Categories")]
    [Category("ImportCategories")]
    Categories = 2,

    [Description("Shop")]
    [Category("ShopSettingTabs")]
    Shop = 0
}

public abstract class SettingsModelBase
{
    public Guid Guid { get; set; } = Guid.NewGuid();

    public abstract TabType Tab { get; }

    public Guid ShopGuid { get; set; }

    public int Id { get; set; }

    public int ShopId { get; set; }

    public string Name { get; set; }

    public virtual void Update(SettingsModelBase source)
    {
        Name = source.Name;
    }

    public override string ToString()
    {
        return $"{nameof(Id)}:{Id};{nameof(Name)}:{Name};{nameof(Guid)}:{Guid};{nameof(ShopGuid)}:{ShopGuid};{nameof(Tab)}:{Tab}";
    }

    public override bool Equals(object? obj)
    {
        return obj != null && obj is SettingsModelBase settingsModel
            && string.Equals(Name, settingsModel.Name, StringComparison.InvariantCultureIgnoreCase)
            && Tab == settingsModel.Tab;
    }
}
