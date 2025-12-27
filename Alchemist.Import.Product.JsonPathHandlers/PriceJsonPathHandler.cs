using Json.CustomSerialization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Alchemist.Import.Product.JsonPathHandlers;

public class PriceJsonPathHandler : IJsonPathHandler
{
    private const string specialChartacterPattern = @"[^\w^\.\,]+";
    private const string literalPattern = @"[a-zA-Z]+";

    public IEnumerable<JsonNode> GetNodes(JsonNode jsonNode, string path)
    {
        var value = jsonNode.ToString().Trim();
        var nodeValue = Regex.Replace(value, specialChartacterPattern, "");
        nodeValue = Regex.Replace(nodeValue, literalPattern, "");
        if(double.TryParse(nodeValue, out var _))
            return [JsonValue.Create(nodeValue)];

        throw new InvalidOperationException($"Invalid price node value {value}");
    }

    public bool IsMatch(JsonNode jsonNode, string path)
    {
        return jsonNode.IsSimpleNode() && path.Contains(".price", StringComparison.InvariantCultureIgnoreCase);
    }
}