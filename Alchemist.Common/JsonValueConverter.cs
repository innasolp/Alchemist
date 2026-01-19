using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Alchemist.Common;

public class JsonValueConverter : JsonConverter<JsonObject>
{
    public override JsonObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return default;

        if (reader.TokenType == JsonTokenType.String)
        {
            var strValue = reader.GetString();
            return JsonSerializer.Deserialize<JsonObject>(strValue, new JsonSerializerOptions(options));
        }

        if(reader.TokenType == JsonTokenType.StartObject)
        {
           var result = JsonSerializer.Deserialize<JsonObject>(ref reader, new JsonSerializerOptions(options));          
                
           if(reader.TokenType == JsonTokenType.EndObject)
                    return result;
        }

        throw new JsonException("Unexpected end of JSON data.");
    }

    public override void Write(Utf8JsonWriter writer, JsonObject value, JsonSerializerOptions options)
    {
        var json = JsonSerializer.Serialize(value, options);    
        writer.WriteRawValue(json);
    }
}