namespace Alchemist.Product.Interfaces;

public interface IShopCategory
{
    public int ShopId { get; set; }

    public string Category { get; set; }

    public int Id { get; set; } 

    public int? ParentId { get; set; }

    public int ItemId { get; set; }

    public string? Url { get; set; }
}