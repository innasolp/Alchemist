using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal static class GuidGenerator
{
    public static Guid ToDeterministicGuid(object source)
    {
        // 1. Сериализуем объект в JSON (детерминированно)
        // Важно: свойства должны быть в одном и том же порядке
        var json = JsonSerializer.Serialize(source, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        // 2. Вычисляем хэш
        using (var md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(json));
            return new Guid(hash);
        }
    }
}