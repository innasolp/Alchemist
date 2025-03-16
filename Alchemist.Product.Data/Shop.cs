namespace Alchemist.Product.Data;

public partial class Shop
{
    public string Name { get; set; } = null!;

    public string Url { get; set; }

    public int Id { get; set; }
    public string? Caption { get; set; }
}
