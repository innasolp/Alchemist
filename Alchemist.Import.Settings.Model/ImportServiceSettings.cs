using Alchemist.Import.Settings.Interfaces;
using DependencyInjection.Interfaces;
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
    public int? ParentSettingsId { get; set; }
    string? IServiceSettings.Value { get => Value?.ToString(); set => Value = JsonSerializer.Deserialize<JsonObject>(value); }
}
