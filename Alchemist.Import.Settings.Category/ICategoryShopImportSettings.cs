using Import.Settings.Interfaces;

namespace Alchemist.Import.Settings.Category;

public interface ICategoryShopImportSettings: IShopImportSettings
{
    string CategorySourceUrl { get; set; }
}
