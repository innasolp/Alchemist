namespace Alchemist.Product.Data;

public partial class PurposeComponentGroup
{
    public short ComponentGroupId { get; set; }

    public short PurposeTypeId { get; set; }

    public string? Comment { get; set; }

    public DateTime AddedTs { get; set; }

    public DateTime? UpdatedTs { get; set; }
}
