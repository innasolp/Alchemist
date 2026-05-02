using Alchemist.Import.Category.Interfaces;
using System.Collections.Specialized;

namespace Alchemist.Import.CategoryService.Test.Infrastructure;

public class TestCategory : ICategory
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Url { get; set; }

    public int? ParentId { get; set; }   

    public bool? IsParented { get; set; }

    public List<TestCategory> Children { get; set; } = [];

    IEnumerable<ICategory> ICategory.Children => Children;

    public List<int> ChildrenIds { get; set; } = [];

    public TestCategory? ItemParent { get; set; }

    ICategory? ICategory.ItemParent => ItemParent;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
}
