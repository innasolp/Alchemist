namespace Alchemist.Product.Data;

public partial class ShopCategory
{
    public int ShopId { get; set; }

    public string Category { get; set; } = null!;

    public int Id { get; set; }

    public int? ParentId { get; set; }

    public int ItemId { get; set; }
}
