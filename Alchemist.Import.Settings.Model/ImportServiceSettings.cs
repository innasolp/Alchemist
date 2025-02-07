using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Alchemist.Import.Settings.Model;

public class ImportServiceSettings : IImportServiceSettings
{   
    public string? Name { get; set; }
    public string? ServiceTypeName { get; set; }
    public string? AssemblyPath { get; set; }
    public string? ServiceProviderPath { get; set; }
    public string? ImplementationTypeName { get; set; }
    public int? Id { get; set; }
    public JsonObject? Value { get; set; }

    public static ImportServiceSettings Create(IShopSettings shopSettings)
    {
        var serviceSettings = JsonSerializer.Deserialize<ImportServiceSettings>(shopSettings.JsonValue);
        serviceSettings.Id = shopSettings.Id;
        return serviceSettings;
    }
}
