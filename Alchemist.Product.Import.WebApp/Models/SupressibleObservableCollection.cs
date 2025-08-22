using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Alchemist.Product.Import.WebApp.Models;

public class SuppressibleObservableCollection<T> : ObservableCollection<T>
{
    private bool _suppressNotifications;

    public void SuspendNotifications()
    {
        _suppressNotifications = true;
    }

    public void ResumeNotifications()
    {
        _suppressNotifications = false;
        // Optionally, raise a Reset notification here to update UI after changes
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (!_suppressNotifications)
        {
            base.OnCollectionChanged(e);
        }
    }
}
