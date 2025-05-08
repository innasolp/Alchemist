using Alchemist.Import.Interfaces;
using Microsoft.VisualStudio.Threading;

namespace Alchemist.Import.Category.Interfaces;

public interface IShopCategoryImportService : IImportService
{
    event AsyncEventHandler<NewCategoryEventArgs> NewCategoryLoad;
}
