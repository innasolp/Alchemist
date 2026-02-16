using System.Text.Json.Nodes;
using System.Text.Json;
using Import.Settings.Interfaces;
namespace Alchemist.Import.Settings.Extensions;

public static class JsonExtensions
{  
    public static void DeserializeValueIfNeed(this IJsonNodeValue jsonValue)
    {
        if (jsonValue.ValueObj == null) return;

        if (jsonValue.ValueObj.Value.ValueKind == JsonValueKind.String && jsonValue.ValueObj != null)
        {
            jsonValue.Value = JsonObject.Parse($"{{\"Value\":\"{jsonValue.ValueObj.Value.GetString()}\"}}") as JsonObject;
        }
        else if (jsonValue.ValueObj.Value.ValueKind == JsonValueKind.Object)
        {
            jsonValue.Value = JsonSerializer.Deserialize<JsonObject>(jsonValue.ValueObj.Value);
        }
        else if (jsonValue.ValueObj.Value.ValueKind == JsonValueKind.Array)
        {
            jsonValue.Value = JsonSerializer.Deserialize<JsonArray>(jsonValue.ValueObj.Value);
        }
    }
}