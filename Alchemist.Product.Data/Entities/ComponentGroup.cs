namespace Alchemist.Product.Data;

public partial class ComponentGroup
{
    public string Name { get; set; } = null!;

    public int? ParentGroupId { get; set; }

    public int Id { get; set; }

    public DateTime AddedTs { get; set; }

    public DateTime? UpdatedTs { get; set; }
}
