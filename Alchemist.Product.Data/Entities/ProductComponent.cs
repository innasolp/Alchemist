namespace Alchemist.Product.Data;

public partial class ProductComponent
{
    public long ProductId { get; set; }

    public int ComponentId { get; set; }

    public short SequalNumber { get; set; }

    public DateTime AddedTs { get; set; }

    public DateTime? UpdatedTs { get; set; }

}
