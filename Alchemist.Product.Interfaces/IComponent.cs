namespace Alchemist.Product.Interfaces;

public interface IComponent
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public int? GroupId { get; set; }

    public string? Transcript { get; set; }
}
