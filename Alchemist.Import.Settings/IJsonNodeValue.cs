using System.Text.Json;
using System.Text.Json.Nodes;

namespace Alchemist.Import.Settings;

public interface IJsonNodeValue
{
    JsonNode? Value { get; set; }

    JsonElement? ValueObj { get; set; }
}
