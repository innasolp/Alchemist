using Alchemist.Product.Data;
using Alchemist.Product.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Alchemist.Product.Data;

public partial class AlchemyContext : DbContext
{
    public AlchemyContext()
    {
        Database.EnsureCreated();
    }

    public AlchemyContext(DbContextOptions<AlchemyContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Component> Components { get; set; }

    public virtual DbSet<ComponentGroup> ComponentGroups { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductComponent> ProductComponents { get; set; }

    public virtual DbSet<ProductType> ProductTypes { get; set; }

    public virtual DbSet<PurposeComponentGroup> PurposeComponentGroups { get; set; }

    public virtual DbSet<PurposeType> PurposeTypes { get; set; }

    public virtual DbSet<Shop> Shops { get; set; }

    public virtual DbSet<ShopCategory> ShopCategories { get; set; }

    public virtual DbSet<ShopProduct> ShopProducts { get; set; }

    public virtual DbSet<ShopUrl> ShopUrls { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<ShopProductPrice> ShopProductPrices { get; set; }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseNpgsql(Configuration.GetConnectionString("DbContext"));
    //"Host=localhost;Port=5432;Database=alchemy;Username=postgres;Password=P@ssword");

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("brand_pk");

            entity.ToTable("brand");

            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Comment)
                .HasMaxLength(1024)
                .HasColumnName("comment");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Component>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("component_pk");

            entity.ToTable("component");

            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Transcript)
                .HasMaxLength(255)
                .HasColumnName("transcript");
        });

        modelBuilder.Entity<ComponentGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("component_group_pk");

            entity.ToTable("component_group");

            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.ParentGroupId).HasColumnName("parent_group_id");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("country_pk");

            entity.ToTable("country");

            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Transcript)
                .HasMaxLength(255)
                .HasColumnName("transcript");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("product_pk");

            entity.ToTable("product");

            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.AddedTime).HasColumnName("added_time");
            entity.Property(e => e.Articul)
                .HasMaxLength(63)
                .HasColumnName("articul");
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.InitShopId).HasColumnName("init_shop_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.ProductTypeId).HasColumnName("product_type_id");
            entity.Property(e => e.Transcript)
                .HasMaxLength(255)
                .HasColumnName("transcript");
        });

        modelBuilder.Entity<ProductComponent>(entity =>
        {
            entity.HasKey(e => new { e.ProductId, e.ComponentId }).HasName("product_component_pk");

            entity.ToTable("product_component");

            entity.Property(e => e.ComponentId).HasColumnName("component_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SequalNumber).HasColumnName("sequal_number");


        });

        modelBuilder.Entity<ProductType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_product_type");

            entity.ToTable("product_type");

            entity.Property(e => e.Id)
                // .HasDefaultValueSql("nextval('product_type_id_seq'::regclass)")
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();
            entity.Property(e => e.Name)
                .HasMaxLength(63)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PurposeComponentGroup>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("purpose_component_group");

            entity.Property(e => e.Comment)
                .HasMaxLength(1023)
                .HasColumnName("comment");
            entity.Property(e => e.ComponentGroupId).HasColumnName("component_group_id");
            entity.Property(e => e.PurposeTypeId).HasColumnName("purpose_type_id");
        });

        modelBuilder.Entity<PurposeType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("purpose_type_pk");

            entity.ToTable("purpose_type");

            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shop_pk");

            entity.ToTable("shop");

            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Url)
                .HasMaxLength(1023)
                .HasColumnName("url");
        });

        modelBuilder.Entity<ShopCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shop_category_pk");

            entity.ToTable("shop_category");

            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.Category)
                .HasMaxLength(255)
                .HasColumnName("category");
            entity.Property(e => e.ShopId).HasColumnName("shop_id");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
        });

        modelBuilder.Entity<ShopProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shop_product_pk");

            entity.ToTable("shop_product");

            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.IsActual).HasColumnName("is_actual");
            entity.Property(e => e.ItemId)
                .HasMaxLength(255)
                .HasColumnName("item_id");
            entity.Property(e => e.ApiUrl)
                .HasMaxLength(1023)
                .HasColumnName("api_url");
            entity.Property(e => e.ItemUrl)
                .HasMaxLength(1023)
                .HasColumnName("item_url");
            entity.Property(e => e.LastUpdate).HasColumnName("last_update");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ShopId).HasColumnName("shop_id");
        });

        modelBuilder.Entity<ShopUrl>(entity =>
        {
            entity.HasKey(e => e.ShopId).HasName("shop_url_pk");

            entity.ToTable("shop_url");

            entity.Property(e => e.ShopId)
                .ValueGeneratedNever()
                .HasColumnName("shop_id");
            entity.Property(e => e.CategoryUrl)
                .HasMaxLength(1023)
                .HasColumnName("category_url");
            entity.Property(e => e.PageProductCount).HasColumnName("page_product_count");
            entity.Property(e => e.ProductUrl)
                .HasMaxLength(1023)
                .HasColumnName("product_url");
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("currency_pkey");

            entity.ToTable("currency");

            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.FullName)
                .HasMaxLength(128)
                .HasColumnName("full_name");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(16)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ShopProductPrice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shop_product_price_pkey");

            entity.ToTable("shop_product_price");

            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.ShopProductId).HasColumnName("shop_product_id");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.LastUpdate).HasColumnName("last_update");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
