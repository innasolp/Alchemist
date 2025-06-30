using Alchemist.Import.Settings.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Alchemist.Product.Import.WebApp.Models;

public static class JsonExtensions
{
    public static void DeserializeValueIfNeed(this IJsonValue jsonValue)
    {
        if (jsonValue.ValueObj == null) return;

        if (jsonValue.ValueObj.Value.ValueKind == JsonValueKind.String)
        {
            jsonValue.Value = JsonSerializer.Deserialize<JsonObject>(jsonValue.ValueObj.Value.GetString());
        }
        else if (jsonValue.ValueObj.Value.ValueKind == JsonValueKind.Object)
        {
            jsonValue.Value = JsonSerializer.Deserialize<JsonObject>(jsonValue.ValueObj.Value);
        }
    }
}
