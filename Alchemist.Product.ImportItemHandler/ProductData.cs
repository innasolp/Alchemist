using Alchemist.Product.ImportItem.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Background.ImportItems;

internal class ProductData : IProductData
{
    public IShopProduct ShopProduct { get; set; }

    public IProduct Product { get; set; }

    public IProductType ProductType { get; set; }

    public IBrand Brand { get; set; }

    public ICountry? Country { get; set; }

    public ICurrency Currency { get; set; }

    public IEnumerable<IPurposeType> PurposeTypes { get; set; }

    public IEnumerable<IComponent> Components { get; set; }

    public IShopProductPrice ShopProductPrice { get; set; }

    public IShopCategory ShopCategory { get; set; }

    public int ShopId { get; set; }
}
