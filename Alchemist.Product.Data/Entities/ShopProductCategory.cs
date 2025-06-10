namespace Alchemist.Product.Data;

public partial class ShopProductCategory 
{
    public long Id { get; set; }
    public long ShopProductId { get; set; }
    public int ShopCategoryId { get; set; }

    public DateTime AddedTs { get; set; }
    public DateTime? UpdatedTs { get; set; }
}
