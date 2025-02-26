namespace Alchemist.Product.Interfaces;

public static class IntefaceExtensions
{
    public static TBrand To<TBrand>(this IBrand brand)
        where TBrand : class, IBrand,new()
    {
        return new TBrand
        {
            Id = brand.Id,
            Name = brand.Name,
            CountryId = brand.CountryId,
            Comment = brand.Comment
        };
    }

    public static TComponent To<TComponent>(this IComponent component)
        where TComponent : class, IComponent, new()
    {
        return new TComponent
        {
            Id = component.Id,
            Name = component.Name,
            GroupId = component.GroupId,
            Description = component.Description,
            Transcript = component.Transcript,
        };
    }

    public static TComponentGroup To<TComponentGroup>(this IComponentGroup componentGroup)
        where TComponentGroup : class, IComponentGroup, new()
    {
        return new TComponentGroup
        {
            Id = componentGroup.Id,
            Name = componentGroup.Name,
            ParentGroupId = componentGroup.ParentGroupId       
        };
    }

    public static TCountry To<TCountry>(this ICountry country)
        where TCountry : class, ICountry, new()
    {
        return new TCountry
        {
            Id = country.Id,
            Name = country.Name,
            Transcript = country.Transcript
        };
    }

    public static T To<T>(this IProduct @in)
        where T : class, IProduct, new()
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

    public static T To<T>(this IProductComponent @in)
        where T : class, IProductComponent, new()
    {
        return new T
        {
            ProductId = @in.ProductId,
            ComponentId = @in.ComponentId,
            SequalNumber = @in.SequalNumber
        };
    }

    public static T To<T>(this IProductType @in)
        where T : class, IProductType, new()
    {
        return new T
        {
            Id = @in.Id,
            Name = @in.Name
        };
    }

    public static T To<T>(this IPurposeType @in)
        where T : class, IPurposeType, new()
    {
        return new T
        {
            Id = @in.Id,
            Name = @in.Name        
        };
    }

    public static T To<T>(this IPurposeComponentGroup @in)
        where T : class, IPurposeComponentGroup, new()
    {
        return new T
        {
            PurposeTypeId = @in.PurposeTypeId,
            ComponentGroupId = @in.ComponentGroupId,
            Comment = @in.Comment
        };
    }

    public static T To<T>(this IShop @in)
        where T : class, IShop, new()
    {
        return new T
        {
            Id = @in.Id,
            Name = @in.Name,
            Url = @in.Url
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
            ParentId = @in.ParentId
        };
    }
    
    public static T To<T>(this IShopProductCategory @in)
        where T : class, IShopProductCategory, new()
    {
        return new T
        {
            Id = @in.Id,
            ShopCategoryId = @in.ShopCategoryId,
            ShopProductId = @in.ShopProductId
        };
    }

    public static T To<T>(this IShopUrl @in)
        where T : class, IShopUrl, new()
    {
        return new T
        {           
            ShopId = @in.ShopId,
            CategoryUrl = @in.CategoryUrl,
            ProductUrl = @in.ProductUrl,
            PageProductCount = @in.PageProductCount
        };
    }
    
    public static T To<T>(this IShopSettings @in)
        where T : class, IShopSettings, new()
    {
        return new T
        {
            Id = @in.Id,
            Name = @in.Name,
            ShopId = @in.ShopId,
            JsonValue = @in.JsonValue,
            Type = @in.Type,
            IsActual = @in.IsActual
        };
    }

    public static T To<T>(this IShopProduct @in)
        where T : class, IShopProduct, new()
    {
        return new T
        {
            Id = @in.Id,
            ShopId = @in.ShopId,
            ProductId = @in.ProductId,
            ItemId = @in.ItemId,
            ApiUrl = @in.ApiUrl,
            ItemUrl = @in.ItemUrl,
            IsActual = @in.IsActual,
            LastUpdate = @in.LastUpdate
        };
    }

    public static T To<T>(this ICurrency @in)
        where T : class, ICurrency, new()
    {
        return new T
        {
            Id = @in.Id,
            Name = @in.Name,
            FullName = @in.FullName,
            Code = @in.Code
        };
    }

    public static T To<T>(this IShopProductPrice @in)
        where T : class, IShopProductPrice, new()
    {
        return new T
        {
            Id = @in.Id,
            ShopProductId = @in.ShopProductId,
            CurrencyId = @in.CurrencyId,
            LastUpdate = @in.LastUpdate,
            Price = @in.Price,
        };
    }
}
