using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Shop.Interfaces;

namespace Alchemist.Product.BeautyAndHealth;

public class BeautyAndHealthProductData //: IBeautyAndHealthProductData
{
    public ShopProduct ShopProduct { get; init; }

    public Entities.Product Product { get; init; }

    public ProductType ProductType { get; init; }

    public Brand? Brand { get; init; }

    public Country? Country { get; init; }

    public Currency Currency { get; init; }

    public List<PurposeType> PurposeTypes { get; init; }

    public List<Component> Components { get; init; }

    public ShopProductPrice ShopProductPrice { get; init; }

    public ShopCategory ShopCategory { get; init; }

    public string ShopName { get; init; }

    public string ShopUrl { get; init; }
}