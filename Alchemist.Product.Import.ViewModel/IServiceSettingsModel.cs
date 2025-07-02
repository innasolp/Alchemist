using Alchemist.Import.Settings.Interfaces;
using System.Text.Json.Nodes;

namespace Alchemist.Product.Import.Model;

public interface IServiceSettingsModel: IShopSettingsModel, IImportServiceSettings, ISettings
{
    JsonObject? JsonValue { get; set; }

    string? FileName { get; set; }    

    Guid ShopSettingsGuid { get; }
}
