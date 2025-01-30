using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Data;

public partial class Brand : IBrand, IEntity<int> { }

public partial class Component : IComponent, IEntity<int> { }

public partial class ComponentGroup : IComponentGroup { }
public partial class Country : ICountry, IEntity<short> { }
public partial class ProductComponent : IProductComponent { }
public partial class Product : IProduct, IEntity<long> { }
public partial class ProductType : IProductType, IEntity<short> { }
public partial class PurposeType : IPurposeType, IEntity<short> { }
public partial class Shop : IShop, IEntity<int> { }
public partial class ShopCategory : IShopCategory { }
public partial class ShopUrl : IShopUrl { }
public partial class ShopProduct : IShopProduct { }
public partial class PurposeComponentGroup : IPurposeComponentGroup { }

public partial class Currency:IEntity<short>, ICurrency { }

public partial class ShopProductPrice : IShopProductPrice { }

public interface IEntity<TId>
    where TId : struct
{
    TId Id { get; set; }
    string Name { get; set; }
}