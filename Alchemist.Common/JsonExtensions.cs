using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Alchemist.Common;

public static class JsonExtensions
{
    public static Action<JsonTypeInfo> IgnorePropertiesForSerialize(Type type, params string[] ignoreProperties) =>
        typeInfo =>
        {
            if (type.IsAssignableFrom(typeInfo.Type) && typeInfo.Kind == JsonTypeInfoKind.Object)
                // [JsonIgnore] is implemented by setting ShouldSerialize to a function that returns false.
                foreach (var property in
                    typeInfo.Properties.Where(p => ignoreProperties.Any(i => p.Name.Equals(i, StringComparison.InvariantCultureIgnoreCase))))
                {
                    if (property.Get != null)
                        property.ShouldSerialize = (param1, param2) => false;
                }
        };

    public static Action<JsonTypeInfo> SetPropertiesForSerialize(Type type, params string[] properties) =>
        typeInfo =>
        {
            if (type.IsAssignableFrom(typeInfo.Type) && typeInfo.Kind == JsonTypeInfoKind.Object)
                // [JsonIgnore] is implemented by setting ShouldSerialize to a function that returns false.
                foreach (var property in typeInfo.Properties)
                {
                    property.ShouldSerialize = (param1, param2) => property.Get != null && properties.Contains(property.Name);
                }
        };

    public static async Task<T?> ReadFromJsonFileAsync<T>(this string path, JsonSerializerOptions? options = null) where T : class
    {
        using FileStream s = File.OpenRead(path);
        var result = options != null
            ? await JsonSerializer.DeserializeAsync<T>(s, options)
            : await JsonSerializer.DeserializeAsync<T>(s);
        s.Close();
        return await Task.FromResult(result);
    }

    public static T? ReadFromJsonFile<T>(this string path) where T : class
    {
        using FileStream fileStream = File.OpenRead(path);
        T result = JsonSerializer.Deserialize<T>(fileStream);
        fileStream.Close();
        return result;
    }

    public static object? ReadFromJsonFile(this string path, Type returnType)
    {
        using FileStream fileStream = File.OpenRead(path);
        object result = JsonSerializer.Deserialize(fileStream, returnType);
        fileStream.Close();
        return result;
    }

    public static async Task<object?> ReadFromJsonFileAsync(this string path, Type returnType)
    {
        using FileStream s = File.OpenRead(path);
        object result = await JsonSerializer.DeserializeAsync(s, returnType);
        s.Close();
        return await Task.FromResult(result);
    }

    public static T? DeserializeAnonymousType<T>(string json, T anonymousTypeObject, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<T>(json, options);
    }

    public static T? DeserializeAnonymousType<T>(object? obj, T anonymousTypeObject, JsonSerializerOptions? options = null)
    {
        var json = JsonSerializer.Serialize(obj, options);
        return JsonSerializer.Deserialize<T>(json, options);
    }
}
