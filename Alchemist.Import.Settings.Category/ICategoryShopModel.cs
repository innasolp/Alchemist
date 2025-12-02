using Import.Settings.Interfaces;

namespace Alchemist.Import.Settings.Category;

public interface ICategoryShopModel : IImportSource
{
    string CategorySourceUrl { get; set; }
}
