using Alchemist.Import.Interfaces;
using System.Collections.ObjectModel;

namespace Alchemist.Import.Products.Interfaces;

public interface IProductShopModel : IShopModel
{
    string ProductUrl { get; set; }

    string CategoryUrl { get; set; }

    ObservableCollection<IProductShopCategoryModel> Categories { get; }
}
