namespace Alchemist.Import.Category.Interfaces;

public interface ICategory
{
    string Name { get; }

    string Url { get; }

    int Id { get; }    

    string? Description { get; }

    int? ParentId { get; }

    IEnumerable<ICategory> Children { get; }

    public bool? IsParented { get; }

    public ICategory? ItemParent { get; }
}
