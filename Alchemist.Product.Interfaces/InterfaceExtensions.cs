namespace Alchemy.Interfaces;

public static class InterfaceExtensions
{
    public static T Convert<T>(this IProduct @in)
        where T:class, IProduct, new()      
    {
        return new T
        {
            Id = @in.Id,
            Name = @in.Name,
            BrandId = @in.BrandId,
            InitShopId = @in.InitShopId,
            ProductTypeId = @in.ProductTypeId,
            AddedTime = @in.AddedTime,
            Transcript = @in.Transcript,
            Articul = @in.Articul
        };
    }

    public static T Convert<T>(this IShopProduct @in)
        where T : class, IShopProduct, new()
    {
        return new T
        {
            Id = @in.Id,
            ShopId = @in.ShopId,
            ProductId = @in.ProductId,
            ItemId = @in.ItemId,
            ItemUrl = @in.ItemUrl,
            IsActual = @in.IsActual,
            LastUpdate = @in.LastUpdate,
            Price = @in.Price
        };
    }
}
