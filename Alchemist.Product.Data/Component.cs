namespace Alchemist.Product.Data;

public partial class Component
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? GroupId { get; set; }

    public int Id { get; set; }

    public string? Transcript { get; set; }
}
