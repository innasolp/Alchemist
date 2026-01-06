using Alchemist.Product.Interfaces;

namespace Alchemist.Product.BeautyAndHealth;

public interface IBeautyAndHealthProductData
{
    IShopProduct ShopProduct { get; }

    IProduct Product { get; }

    IProductType ProductType { get; }

    IBrand Brand { get; }

    ICountry? Country { get; }

    ICurrency Currency { get; }

    IEnumerable<IPurposeType> PurposeTypes { get; }

    IEnumerable<IComponent> Components { get; }

    IShopProductPrice ShopProductPrice { get; }

    IShopCategory ShopCategory { get; }

    public string ShopName { get; }

    public string ShopUrl { get; }
}