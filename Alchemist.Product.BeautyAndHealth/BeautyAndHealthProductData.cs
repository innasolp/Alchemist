using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Shop.Interfaces;

namespace Alchemist.Product.BeautyAndHealth;

public class BeautyAndHealthProductData : IBeautyAndHealthProductData
{
    public ShopProduct ShopProduct { get; set; }

    public Entities.Product Product { get; set; }

    public ProductType ProductType { get; set; }

    public Brand? Brand { get; set; }

    public Country? Country { get; set; }

    public Currency Currency { get; set; }

    public List<PurposeType> PurposeTypes { get; set; }

    public List<Component> Components { get; set; }

    public ShopProductPrice ShopProductPrice { get; set; }

    public ShopCategory ShopCategory { get; set; }

    public string ShopName { get; set; }

    public string ShopUrl { get; set; }

    IShopProduct IBeautyAndHealthProductData.ShopProduct => ShopProduct;

    IProduct IBeautyAndHealthProductData.Product => Product;

    IProductType IBeautyAndHealthProductData.ProductType => ProductType;

    IBrand? IBeautyAndHealthProductData.Brand => Brand;

    ICountry? IBeautyAndHealthProductData.Country => Country;

    ICurrency IBeautyAndHealthProductData.Currency => Currency;

    IEnumerable<IPurposeType> IBeautyAndHealthProductData.PurposeTypes => PurposeTypes;

    IEnumerable<IComponent> IBeautyAndHealthProductData.Components => Components;

    IShopProductPrice IBeautyAndHealthProductData.ShopProductPrice => ShopProductPrice;

    IShopCategory IBeautyAndHealthProductData.ShopCategory => ShopCategory;
}