using Alchemist.Import.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Logging;

public interface IImportServiceLogFactory
{
    ILogger<T> GetLogger<T>(ILogger<T> logger, IShopModel shopModel, IShopImportSettings shopImportSettings);
}
