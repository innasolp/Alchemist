using Alchemist.Import.Interfaces;

namespace Alchemist.Import.Category.Interfaces;

public interface ICategoryShopModel : IShopItem
{
    string CategorySourceUrl { get; set; }
}
