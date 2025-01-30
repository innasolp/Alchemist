using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Entities;

public class ComponentGroup:IComponentGroup
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int? ParentGroupId { get; set; }
}
