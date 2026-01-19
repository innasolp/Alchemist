namespace ShopSettings.Interfaces;

public static class Extensions
{
    public static T To<T>(this IShopSettings @in)
        where T : class, IShopSettings, new()
    {
        return new T
        {
            Id = @in.Id,
            Name = @in.Name,
            ShopId = @in.ShopId,
            JsonValue = @in.JsonValue,
            ParentSettingsId = @in.ParentSettingsId,
            Type = @in.Type,
            IsActual = @in.IsActual
        };
    }
}