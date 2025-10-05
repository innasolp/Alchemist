using Alchemist.Import.Interfaces;
using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Import.Factory.BrowserService;

public interface ILoaderServiceFactory
{
    ILoaderService Create(string name, IShopImportSettings shopImportSettings);
}
