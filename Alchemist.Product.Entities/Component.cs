namespace Alchemist.Product.Entities;

public class Component: Interfaces.IComponent
{
    public int Id { get; set; }

    public string Name { get; set; }
    public string? Transcript { get; set; }
    public string? Description { get; set; }

    public int? GroupId { get; set; }
}
