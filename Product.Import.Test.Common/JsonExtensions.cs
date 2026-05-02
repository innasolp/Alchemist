using System.Text.Json;

namespace Product.Import.Json.Test.Common;

public static class JsonExtensions
{
    public static T? ReadFromJsonFile<T>(this string path) where T : class
    {
        using FileStream fileStream = File.OpenRead(path);
        T result = JsonSerializer.Deserialize<T>(fileStream);
        fileStream.Close();
        return result;
    }
}