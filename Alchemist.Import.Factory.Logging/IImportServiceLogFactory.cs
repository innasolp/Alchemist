using Alchemist.Import.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Factory.Logging;

public interface IImportServiceLogFactory
{
    ILogger<T> GetLogger<T>(ILogger<T> logger, IShopItem shopModel, IShopImportSettings shopImportSettings);
}
