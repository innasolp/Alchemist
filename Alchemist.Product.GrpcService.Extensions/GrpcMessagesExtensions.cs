using Alchemist.Product.Interfaces;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Alchemist.Product.GrpcService.Extensions;

public static class GrpcMessagesExtensions
{
    public static TProductMessage ToMessage<TProductMessage>(this IProduct product)
    where TProductMessage : class, IProductMessage, new()
    {
        return new TProductMessage
        {
            Name = product.Name,
            Producttypeid = product.ProductTypeId,
            Brandid = product.BrandId,
            Initshopid = product.InitShopId,
            Transcript = product.Transcript,
            Addedts = product.AddedTime.ToUniversalTime().ToTimestamp(),
            Articul = product.Articul
        };
    }

    public static TShopProductMessage ToMessage<TShopProductMessage>(this IShopProduct shopProduct)
    where TShopProductMessage : class, IShopProductMessage, new()
    {
        return new TShopProductMessage
        {
            Shopid = shopProduct.ShopId,
            Productid = shopProduct.ProductId,
            Itemid = shopProduct.ItemId,
            Itemurl = shopProduct.ItemUrl,
            Apiurl = shopProduct.ApiUrl,
            Lastupdate = shopProduct.LastUpdate.ToUniversalTime().ToTimestamp(),
            Isactual = shopProduct.IsActual
        };
    }
    public static TProduct FromMessage<TProduct>(this IProductMessage message)
    where TProduct : class, IProduct, new()
    {
        return new TProduct
        {
            Name = message.Name,
            ProductTypeId = (short)message.Producttypeid,
            BrandId = message.Brandid,
            InitShopId = message.Initshopid,
            AddedTime = message.Addedts.ToDateTime(),
            Articul = message.Articul
        };
    }

    public static TShopProduct FromMessage<TShopProduct>(this IShopProductMessage message)
    where TShopProduct : class, IShopProduct, new()
    {
        return new TShopProduct
        {
            ShopId = message.Shopid,
            ProductId = message.Productid,
            ItemId = message.Itemid,
            ApiUrl = message.Apiurl,
            ItemUrl = message.Itemurl,
            LastUpdate = message.Lastupdate.ToDateTime(),
            IsActual = message.Isactual
        };
    }

    public static TShopProductCategory FromMessage<TShopProductCategory>(this IShopProductCategoryMessage message)
    where TShopProductCategory : class, IShopProductCategory, new()
    {
        return new TShopProductCategory
        {
            ShopProductId = message.Shopproductid,
            ShopCategoryId = message.Shopcategoryid,
        };
    }

    public static TComponent FromMessage<TComponent>(this IComponentMessage message)
    where TComponent : class, IComponent, new()
    {
        return new TComponent
        {
            Name = message.Name,
            Description = message.Description,
            GroupId = message.Groupid,
            Transcript = message.Transcript
        };
    }
    public static TMessage ToMessage<TMessage>(this IComponent component)
    where TMessage : class, IComponentMessage, new()
    {
        return new TMessage
        {
            Name = component.Name,
            Description = component.Description,
            Groupid = component.GroupId,
            Transcript = component.Transcript
        };
    }

    public static TShopProductPrice FromMessage<TShopProductPrice>(this IShopProductPriceMessage message)
    where TShopProductPrice : class, IShopProductPrice, new()
    {
        return new TShopProductPrice
        {
            ShopProductId = message.Shopproductid,
            Price = message.Price,
            CurrencyId = message.Currencyid,
            LastUpdate = message.Lastupdate.ToDateTime()
        };
    }

    public static TMessage ToMessage<TMessage>(this IShopProductPrice shopProductPrice)
    where TMessage : class, IShopProductPriceMessage, new()
    {
        return new TMessage
        {
            Shopproductid = shopProductPrice.ShopProductId,
            Price = shopProductPrice.Price,
            Currencyid = shopProductPrice.CurrencyId,
            Lastupdate = shopProductPrice.LastUpdate.ToUniversalTime().ToTimestamp()
        };
    }

    public static TShopProductCategoryMessage ToMessage<TShopProductCategoryMessage>(this IShopProductCategory shopProductCategory)
    where TShopProductCategoryMessage : class, IShopProductCategoryMessage, new()
    {
        return new TShopProductCategoryMessage
        {
            Shopproductid = shopProductCategory.ShopProductId,
            Shopcategoryid = shopProductCategory.ShopCategoryId
        };
    }

    public static TMessage ToMessage<TMessage>(this IProductComponent entity)
        where TMessage:IProductComponentMessage,new()        
    {
        return new TMessage
        {
            Componentid = entity.ComponentId,
            Productid = entity.ProductId,
            SequalNumber = entity.SequalNumber
        };
    }

    public static TProductComponent FromMessage<TProductComponent>(this IProductComponentMessage message)
    where TProductComponent : class, IProductComponent, new()
    {
        return new TProductComponent
        {
            ProductId = message.Productid,
            ComponentId = message.Componentid,
            SequalNumber = (short)message.SequalNumber
        };
    }

    public static TMessage ToMessage<TMessage>(this ICurrency entity)
        where TMessage : ICurrencyMessage, new()
    {
        return new TMessage
        {
            Name = entity.Name,
            Fullname = entity.FullName,
            Code = entity.Code
        };
    }

    public static TCurrency FromMessage<TCurrency>(this ICurrencyMessage message)
    where TCurrency : class, ICurrency, new()
    {
        return new TCurrency
        {
            Name = message.Name,
            FullName = message.Fullname,
            Code = (short?)message.Code
        };
    }

    public static Task<TListReply> ToListReply<TListReply, TReply, TEntity>(this
        List<TEntity> entities,
        Func<TEntity, TReply> createReplyItem)
        where TListReply : class, IListReply<TReply>, IMessage, new()
        where TReply : class, IMessage, new()
    {
        var list = entities.Select(item => createReplyItem(item)).ToList();
        var listReply = new TListReply();
        listReply.Repeated.AddRange(list);
        return Task.FromResult(listReply);
    }

    public static Task<List<TEntity>> FromListReply<TListReply, TReply, TEntity>(this
        TListReply listReply,
        Func<TReply, TEntity> createEntity)
        where TListReply : class, IListReply<TReply>, IMessage, new()
        where TReply : class, IMessage, new()
    {
        var list = listReply.Repeated.Select(item => createEntity(item)).ToList();        
        return Task.FromResult(list);
    }
}
