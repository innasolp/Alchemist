namespace Alchemist.Product.Interfaces;

public interface IComponentGroup
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int? ParentGroupId { get; set; }
}
