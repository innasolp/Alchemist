namespace Alchemist.Product.Data;

public partial class ShopProduct
{
    public long ProductId { get; set; }

    public int ShopId { get; set; }

    public bool? IsActual { get; set; }

    public string ApiUrl { get; set; }

    public long Id { get; set; }

    public string ItemId { get; set; }

    public string ItemUrl { get; set; }

    public DateTime AddedTs { get; set; }

    public DateTime? UpdatedTs { get; set; }
}
