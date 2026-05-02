using Shop.Interfaces;

namespace Alchemist.Product.Entities;

public class ShopCategory : IShopCategory
{
    public int ShopId { get; set; }

    public string Category { get; set; }

    public int Id { get; set; }

    public int? ParentId { get; set; }

    public int ItemId { get; set; }

    public string? Url { get; set; }

    public string Path { get; set; } = "";
}
