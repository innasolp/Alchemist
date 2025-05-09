using Alchemist.Import.Interfaces;
using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Import.Factory;

public interface IShopImportServiceFactory
{
    Type ServiceImplementationType { get; }

    IImportService Create(IShopModel shopModel, IShopImportSettings shopImportSettings);
}
