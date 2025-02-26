using System.ComponentModel;

namespace Alchemist.Product.Import.Model.Infrastructure;

public enum TabType
{
    [Description("Products")]
    [Category("ImportProducts")]
    Products = 0,

    [Description("Categories")]
    [Category("ImportCategories")]
    Categories = 1,

    [Description("Shop")]
    [Category("ShopSettingTabs")]
    Shop = 2
}

public abstract class SettingsModelBase
{
    public abstract TabType Tab { get; }

    public Guid ShopGuid { get; set; }

    public int Id { get; set; }

    public int ShopId { get; set; }

    public string Name { get; set; }

    public virtual void Update(SettingsModelBase source)
    {
        Name = source.Name;
    }
}
