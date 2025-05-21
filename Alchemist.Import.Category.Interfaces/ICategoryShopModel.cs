using Alchemist.Import.Interfaces;

namespace Alchemist.Import.Category.Interfaces;

public interface ICategoryShopModel : IShopModel
{
    string CategorySourceUrl { get; set; }
}
