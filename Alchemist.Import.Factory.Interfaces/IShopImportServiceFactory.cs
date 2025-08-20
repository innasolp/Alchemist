using Alchemist.Import.Interfaces;
using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Import.Service.Factory.Interfaces;

public interface IShopImportServiceFactory
{
    Type ServiceImplementationType { get; }

    IImportService Create(IShopItem shopModel, IShopImportSettings shopImportSettings);
}
