using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Alchemist.Product.Interfaces;

//todo move to other project
public interface IShopUrlModel : IShopUrl, INotifyPropertyChanged
{   
    public string Name { get; set; }

    public string Url { get; set; }

    ObservableCollection<IShopCategory> Categories { get; }
}
