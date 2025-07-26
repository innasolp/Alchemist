using Alchemist.Import.Interfaces;
using System.Collections.Specialized;

namespace Alchemist.Import.Category.Interfaces;

public interface ICategory : IItem, INotifyCollectionChanged
{
    int Id { get; }    

    string Description { get; }

    int? ParentId { get; }

    IEnumerable<ICategory> Children { get; }

    IEnumerable<int> ChildrenIds { get; }

    public bool? IsParented { get; }

    public ICategory? ItemParent { get; }
}
