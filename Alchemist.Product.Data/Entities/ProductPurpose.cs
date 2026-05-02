namespace Alchemist.Product.Data;

public partial class ProductPurpose
{
    public long ProductId { get; set; }

    public short PurposeTypeId { get; set; }

    public DateTime AddedTs { get; set; }

    public DateTime? UpdatedTs { get; set; }
}
