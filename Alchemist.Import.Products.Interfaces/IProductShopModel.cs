using Alchemist.Import.Interfaces;
using System.Collections.ObjectModel;

namespace Alchemist.Import.Products.Interfaces;

public interface IProductShopModel : IShopItem
{
    string ProductUrl { get; set; }

    string CategoryUrl { get; set; }

    ObservableCollection<IProductShopCategory> Categories { get; }
}
