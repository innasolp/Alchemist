using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alchemist.Product.Data;

public partial class Brand : IEntity<int>, IAddedTsEnity { }

public partial class Component : IEntity<int>, IAddedTsEnity { }

public partial class ComponentGroup :  IAddedTsEnity, IUpdatedTsEntity{ }
public partial class Country : IEntity<short>, IAddedTsEnity { }
public partial class ProductComponent : IAddedTsEnity, IUpdatedTsEntity { }
public partial class Product : IEntity<long>, IAddedTsEnity, IUpdatedTsEntity { }
public partial class ProductType : IEntity<short> , IAddedTsEnity{ }
public partial class PurposeType : IEntity<short>, IAddedTsEnity { }
public partial class Shop : IEntity<int>, IAddedTsEnity, IUpdatedTsEntity { }
public partial class ShopCategory : IAddedTsEnity, IUpdatedTsEntity, IMaterialPathEntity { }
public partial class ShopProduct :  IAddedTsEnity, IUpdatedTsEntity { }
public partial class ShopProductCategory : IAddedTsEnity, IUpdatedTsEntity { }
public partial class PurposeComponentGroup : IAddedTsEnity, IUpdatedTsEntity { }
public partial class ProductPurpose : IAddedTsEnity, IUpdatedTsEntity { }

public partial class ShopSettings : IAddedTsEnity, IUpdatedTsEntity { }

public partial class Currency:IEntity<short>, IAddedTsEnity { }

public partial class ShopProductPrice :  IAddedTsEnity, IUpdatedTsEntity { }

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