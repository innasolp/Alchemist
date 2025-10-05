using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Model.ShopSettings;
using System.Text.Json.Nodes;

namespace Alchemist.Product.Import.Model;

public interface IServiceSettingsModel: IShopSettingsModel, IServiceSettings
{
    JsonObject? JsonValue { get; set; }

    Guid ShopSettingsGuid { get; }
}
