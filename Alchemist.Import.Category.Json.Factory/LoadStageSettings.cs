using System.Text.Json.Nodes;

namespace Alchemist.Import.Factory.Category;

internal class LoadStageSettings
{
    public string Type { get; set; } = string.Empty;

    public JsonObject CategoryLoadOptions { get; set; } = [];
}
