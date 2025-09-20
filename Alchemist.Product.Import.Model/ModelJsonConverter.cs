using Alchemist.Common;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.Model;

internal class ModelJsonConverter<T> : JsonConverter<T>   
    where T : class, IModel
{
    public ModelJsonConverter() { }

    public ModelJsonConverter(IDictionary<string, object> propertyDefaultValues) { _propertyDefaultValues = propertyDefaultValues; }

    private readonly IDictionary<string, object> _propertyDefaultValues = new Dictionary<string, object>();

    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(T).IsAssignableFrom(typeToConvert); 
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonDocument = JsonNode.Parse(ref reader);
        var jsonObject = jsonDocument.AsObject();
        
        foreach(var prop in _propertyDefaultValues)
        {
            if (jsonDocument[prop.Key] == null)
            {
                if(prop.Value.GetType().IsNumericType())
                    jsonObject[prop.Key] = JsonNode.Parse(prop.Value.ToString());
                else
                {
                    jsonObject[prop.Key] = JsonNode.Parse($"\"{prop.Value.ToString()}\"");
                }
            }
        }

        return jsonObject.Deserialize(typeToConvert) as T ?? null;

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var jsonDoc = JsonSerializer.SerializeToDocument(value, value.GetType(),
            new JsonSerializerOptions { RespectRequiredConstructorParameters = true, IgnoreReadOnlyProperties = false } );
        jsonDoc.WriteTo(writer);
    }
}
