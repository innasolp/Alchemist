using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alchemist.Product.ImportSettingsWebApp.Infrastructure;

public class EmptyGuidConverter : JsonConverter<Guid>
{
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string guidString = reader.GetString();
            if (string.IsNullOrEmpty(guidString) || guidString.Equals("null", StringComparison.InvariantCultureIgnoreCase))
            {
                return Guid.NewGuid();
            }
            return Guid.Parse(guidString);
        }
        // Handle other token types or throw an exception if not a string
        throw new JsonException("Expected string for Guid.");
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
