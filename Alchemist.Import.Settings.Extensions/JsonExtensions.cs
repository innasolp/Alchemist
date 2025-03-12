using Alchemist.Import.Settings.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Alchemist.Import.Settings.Extensions;

public static class JsonExtensions
{
    public static Action<JsonTypeInfo> IgnorePropertiesForSerialize(Type type, params string[] properties) =>
        typeInfo =>
        {
            if (type.IsAssignableFrom(typeInfo.Type) && typeInfo.Kind == JsonTypeInfoKind.Object)
                // [JsonIgnore] is implemented by setting ShouldSerialize to a function that returns false.
                foreach (var property in typeInfo.Properties.Where(p => properties.Contains(p.Name)))
                {
                    if (property.Get != null)
                        property.ShouldSerialize = (param1,param2)=>false ;
                }
        };

    public static Action<JsonTypeInfo> SetPropertiesForSerialize(Type type, params string[] properties) =>
        typeInfo =>
        {
            if (type.IsAssignableFrom(typeInfo.Type) && typeInfo.Kind == JsonTypeInfoKind.Object)
                // [JsonIgnore] is implemented by setting ShouldSerialize to a function that returns false.
                foreach (var property in typeInfo.Properties)
                {
                    property.ShouldSerialize =  (param1, param2) => property.Get != null && properties.Contains(property.Name);
                }
        };
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
