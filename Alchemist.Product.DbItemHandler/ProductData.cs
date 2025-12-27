using Alchemist.Product.DataItem.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.DbItemHandler;

internal class ProductData : IProductData
{
    public ShopProduct ShopProduct { get; set; }

    public Entities.Product Product { get; set; }

    public ProductType ProductType { get; set; }

    public Brand Brand { get; set; }

    public Country? Country { get; set; }

    public Currency Currency { get; set; }

    public List<PurposeType> PurposeTypes { get; set; }

    public List<Component> Components { get; set; }

    public ShopProductPrice ShopProductPrice { get; set; }

    public ShopCategory ShopCategory { get; set; }

    public string ShopName { get; set; }

    public string ShopUrl { get; set; }

    IShopProduct IProductData.ShopProduct => ShopProduct;

    IProduct IProductData.Product => Product;

    IProductType IProductData.ProductType => ProductType;

    IBrand IProductData.Brand => Brand;

    ICountry? IProductData.Country => Country;

    ICurrency IProductData.Currency => Currency;

    IEnumerable<IPurposeType> IProductData.PurposeTypes => PurposeTypes;

    IEnumerable<IComponent> IProductData.Components => Components;

    IShopProductPrice IProductData.ShopProductPrice => ShopProductPrice;

    IShopCategory IProductData.ShopCategory => ShopCategory;
}
