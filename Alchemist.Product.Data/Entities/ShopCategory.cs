namespace Alchemist.Product.Data;

public partial class ShopCategory
{
    public int ShopId { get; set; }

    public string Category { get; set; } = null!;

    public int Id { get; set; }

    public int? ParentId { get; set; }

    public int ItemId { get; set; }

    public DateTime AddedTs { get; set; }

    public DateTime? UpdatedTs { get; set; }

    public string? Url { get; set; }
}
