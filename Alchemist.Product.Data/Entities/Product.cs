namespace Alchemist.Product.Data;

public partial class Product
{
    public string Name { get; set; } = null!;

    public string? Transcript { get; set; }

    public int? BrandId { get; set; }

    public string? Articul { get; set; }

    public int? InitShopId { get; set; }

    public DateTime AddedTime { get; set; }

    public short ProductTypeId { get; set; }

    public long Id { get; set; }

    public DateTime AddedTs { get; set; }

    public DateTime? UpdatedTs { get; set; }
}
