using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Product.Import.Model;

public interface IShopSettingsModel : ISettingsModel, ISettings
{
    string? FileName { get; set; }
}
