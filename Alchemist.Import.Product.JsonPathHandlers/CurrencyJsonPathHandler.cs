using Json.CustomSerialization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Alchemist.Import.Product.JsonPathHandlers;

public class CurrencyJsonPathHandler : IJsonPathHandler
{
    private const string specialChartacterPatternFormat = @"[^\w {0}]+";
    private const string numericPattern = @"[0-9]+";
    private readonly string[] currencyLiterals = ["₽"];

    public IEnumerable<JsonNode> GetNodes(JsonNode jsonNode, string path)
    {
        var value = jsonNode.ToString().Trim();

        var specialCharacterPattern = string.Format(specialChartacterPatternFormat, $"^\\{string.Join('\\', currencyLiterals)}");

        var nodeValue = Regex.Replace(value, specialCharacterPattern, "");
        nodeValue = Regex.Replace(nodeValue, numericPattern, "");

        if (!string.IsNullOrEmpty(nodeValue))
            return [JsonValue.Create(nodeValue)];

        throw new InvalidOperationException($"Invalid currency node value {value}");
    }

    public bool IsMatch(JsonNode jsonNode, string path)
    {
        return jsonNode.IsSimpleNode() && path.Contains(".currency");
    }
}