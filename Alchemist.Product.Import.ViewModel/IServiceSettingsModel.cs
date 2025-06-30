using Alchemist.Import.Settings.Interfaces;
using System.Text.Json.Nodes;

namespace Alchemist.Product.Import.Model;

public interface IServiceSettingsModel: IShopSettingsModel, IImportServiceSettings, ISettings
{
    new int? ParentSettingsId { get; set; }   

    JsonObject? JsonValue { get; set; }

    string? FileName { get; set; }    

    Guid ShopSettingsGuid { get; set; }
}
