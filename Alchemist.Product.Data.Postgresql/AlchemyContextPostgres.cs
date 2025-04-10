using Microsoft.EntityFrameworkCore;

namespace Alchemist.Product.Data.Postgresql;

internal class AlchemyContextPostgres: AlchemyContext
{
    public AlchemyContextPostgres()
    {
    }

    public AlchemyContextPostgres(DbContextOptions<AlchemyContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        });

        modelBuilder.Entity<Component>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        });

        modelBuilder.Entity<ComponentGroup>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();            
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();            
        });

        modelBuilder.Entity<ProductType>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();            
        });

        modelBuilder.Entity<PurposeType>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        });

        modelBuilder.Entity<Shop>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        });

        modelBuilder.Entity<ShopCategory>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        });

        modelBuilder.Entity<ShopProduct>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        });

        modelBuilder.Entity<ShopProductCategory>(entity =>
        {
           entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        });

        modelBuilder.Entity<ShopSettings>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        });

        modelBuilder.Entity<ShopProductPrice>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        });
    }

}
