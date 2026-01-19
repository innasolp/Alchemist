namespace Shop.Interfaces;

public static class  Extensions
{
    public static T To<T>(this IShop @in)
        where T : class, IShop, new()
    {
        return new T
        {
            Id = @in.Id,
            Name = @in.Name,
            Url = @in.Url,
            Caption = @in.Caption
        };
    }

    public static T To<T>(this IShopCategory @in)
        where T : class, IShopCategory, new()
    {
        return new T
        {
            Id = @in.Id,
            Category = @in.Category,
            ShopId = @in.ShopId,
            ItemId = @in.ItemId,
            ParentId = @in.ParentId,
            Url = @in.Url
        };
    }
}