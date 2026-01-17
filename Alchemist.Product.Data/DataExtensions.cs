using Alchemist.Product.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alchemist.Product.Data;

public partial class Brand : IBrand, IEntity<int>, IAddedTsEnity { }

public partial class Component : IComponent, IEntity<int>, IAddedTsEnity { }

public partial class ComponentGroup : IComponentGroup , IAddedTsEnity, IUpdatedTsEntity{ }
public partial class Country : ICountry, IEntity<short>, IAddedTsEnity { }
public partial class ProductComponent : IProductComponent, IAddedTsEnity, IUpdatedTsEntity { }
public partial class Product : IProduct, IEntity<long>, IAddedTsEnity, IUpdatedTsEntity { }
public partial class ProductType : IProductType, IEntity<short> , IAddedTsEnity{ }
public partial class PurposeType : IPurposeType, IEntity<short>, IAddedTsEnity { }
public partial class Shop : IShop, IEntity<int>, IAddedTsEnity, IUpdatedTsEntity { }
public partial class ShopCategory : IShopCategory, IAddedTsEnity, IUpdatedTsEntity { }
public partial class ShopProduct : IShopProduct, IAddedTsEnity, IUpdatedTsEntity { }
public partial class ShopProductCategory : IShopProductCategory, IAddedTsEnity, IUpdatedTsEntity { }
public partial class PurposeComponentGroup : IPurposeComponentGroup, IAddedTsEnity, IUpdatedTsEntity { }
public partial class ProductPurpose : IProductPurpose, IAddedTsEnity, IUpdatedTsEntity { }

public partial class ShopSettings : IShopSettings, IAddedTsEnity, IUpdatedTsEntity
{
    Interfaces.ShopSettingType IShopSettings.Type { get => (Interfaces.ShopSettingType)(int)Type; set => Type = (ShopSettingType)(int)value; }
}

public partial class Currency:IEntity<short>, ICurrency, IAddedTsEnity { }

public partial class ShopProductPrice : IShopProductPrice, IAddedTsEnity, IUpdatedTsEntity { }

public interface IEntity
{
    string Name { get; set; }
}

public interface IEntity<TId> : IEntity
    where TId : struct
{
    TId Id { get; set; }
}

public static class ContextExtensions
{
    public static void SetAddedTsColumn<T>(this EntityTypeBuilder<T> entity)
        where T : class, IAddedTsEnity
    {
        entity.Property(e => e.AddedTs).HasColumnName("added_ts");
    }

    public static void SetChangedTsColumns<T>(this EntityTypeBuilder<T> entity)
        where T : class, IAddedTsEnity, IUpdatedTsEntity
    {
        entity.Property(e => e.AddedTs).HasColumnName("added_ts");
        entity.Property(e => e.UpdatedTs).HasColumnName("update_ts");
    }
}