using System.Data;

namespace Alchemist.Product.Import.WebApp.Infrastructure;

public enum SettingsType
{
    Products=0,
    Categories=1,
    Shop=2
}

public abstract class SettingsModelBase
{    
    public abstract SettingsType Type { get; }

    public int ShopId { get; set; } 

    public int Id { get; set; }

    public string Name { get; set; }

    public virtual void Update(SettingsModelBase source)
    {
        Name = source.Name;
    }
}
