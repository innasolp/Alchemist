using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Shop.Interfaces;
using Microsoft.VisualStudio.Threading;

namespace Alchemist.Import.Products.Service;

public interface IShopProductImportService : IShopImportService
{
    event AsyncEventHandler<ItemHandledEventArgs>? ItemHandled;
}
