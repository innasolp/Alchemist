namespace Alchemist.Product.Import.WebApp.Infrastructure;

public enum TabType
{
    Products=0,
    Categories=1,
    Shop=2
}

public abstract class SettingsModelBase
{    
    public abstract TabType Tab { get; }

    public int ShopId { get; set; } 

    public int Id { get; set; }

    public string Name { get; set; }

    public virtual void Update(SettingsModelBase source)
    {
        Name = source.Name;
    }
}
