using Alchemist.Import.Interfaces;

namespace Alchemist.Import.Factory;

public interface IShopImportServiceFactory
{
    IImportService Create(IShopModel shopModel);
}
