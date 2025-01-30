using Alchemist.Import.Shop.Interfaces;
using Microsoft.VisualStudio.Threading;

namespace Alchemist.Import.Category.Interfaces;

public interface IShopCategoryImportService : IShopImportService
{
    event AsyncEventHandler<NewCategoryEventArgs> NewCategoryLoad;
}
