namespace Alchemist.Product.Data;

public partial class Brand
{
    public string Name { get; set; } = null!;

    public short? CountryId { get; set; }

    public string? Comment { get; set; }

    public int Id { get; set; }

    public DateTime AddedTs { get; set; }
}
