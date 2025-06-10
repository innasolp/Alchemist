using Microsoft.EntityFrameworkCore;

namespace Alchemist.Product.Data;

public interface IAlchemyContext
{
    DbSet<Brand> Brands { get; set; }

    DbSet<Component> Components { get; set; }

    DbSet<ComponentGroup> ComponentGroups { get; set; }

    DbSet<Country> Countries { get; set; }

    DbSet<Product> Products { get; set; }

    DbSet<ProductComponent> ProductComponents { get; set; }

    DbSet<ProductType> ProductTypes { get; set; }

    DbSet<PurposeComponentGroup> PurposeComponentGroups { get; set; }

    DbSet<PurposeType> PurposeTypes { get; set; }

    DbSet<Shop> Shops { get; set; }

    DbSet<ShopCategory> ShopCategories { get; set; }

    DbSet<ShopProduct> ShopProducts { get; set; }

    DbSet<ShopProductCategory> ShopProductCategories { get; set; }

    DbSet<ShopSettings> ShopSettings { get; set; }

    DbSet<Currency> Currencies { get; set; }

    DbSet<ShopProductPrice> ShopProductPrices { get; set; }

    DbSet<ProductPurpose> ProductPurposes { get; set; }
}
