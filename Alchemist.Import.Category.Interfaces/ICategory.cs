using System.Collections.Specialized;

namespace Alchemist.Import.Category.Interfaces;

public interface ICategory : INotifyCollectionChanged
{
    int Id { get; }

    string Name { get; }

    string Description { get; }

    string Url { get; }

    int? ParentId { get; }

    IEnumerable<ICategory> Children { get; }

    IEnumerable<int> ChildrenIds { get; }

    public bool? IsParented { get; }

    public ICategory? ItemParent { get; }
}
