using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Product.ImportSettingsWebApp.Models;

public class CategoryUrlModel : ICategoryUrl
{
    public int Item { get; set; }

    public string Url { get; set; }

    public int ShopId { get; set; }

    public Guid Guid { get; set; } = Guid.NewGuid();
}
