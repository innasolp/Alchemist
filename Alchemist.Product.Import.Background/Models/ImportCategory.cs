using Alchemist.Common;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Product.Interfaces;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.Background.Models;

internal class ImportCategory : IImportCategoryItem
{
    public int Id { get; set; }
    public int ItemId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Url { get; set; }

    public int? ItemParentId { get; set; }

    public bool? IsParented { get; set; }

    [JsonIgnore]
    public ICategory? ItemParent { get; set; }

    int ICategory.Id => ItemId;

    public int ShopId { get; set; }
    public string Category { get; set; }

    int? ICategory.ParentId => ItemParentId;

    public int? ParentId { get; set; }

    public ResultStatus Status { get; set; }

    [JsonIgnore]
    public ObservableCollection<ICategory> Children { get; } = [];

    [JsonIgnore]
    IEnumerable<ICategory> ICategory.Children => Children;

    IEnumerable<int> ICategory.ChildrenIds => throw new NotImplementedException();

    public bool Success { get; set; }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public ImportCategory() => Children.CollectionChanged += ChildrenCollectionChanged;

    public ImportCategory(IShopCategory shopCategory,
                          ICategory itemCategory,
                          ResultStatus status)
    {
        Id = shopCategory.Id;
        ItemId = shopCategory.ItemId;
        ParentId = shopCategory.ParentId;
        Category = shopCategory.Category;
        ShopId = shopCategory.ShopId;

        Url = itemCategory.Url;
        Name = itemCategory.Name;
        ItemParentId = itemCategory.ParentId;
        IsParented = itemCategory.IsParented;
        ItemParent = itemCategory.ItemParent;
        itemCategory.Children.ToList().ForEach(Children.Add);

        Status = status;

        Children.CollectionChanged += ChildrenCollectionChanged;
    }

    private void ChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }

}
