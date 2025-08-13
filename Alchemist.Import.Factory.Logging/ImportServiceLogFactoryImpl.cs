using Alchemist.Import.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Logging;

internal class ImportServiceLogFactoryImpl(Func<ILogger, IShopItem, IShopImportSettings, ILogger> loggerInterception) : IImportServiceLogFactory
{
    private readonly Func<ILogger, IShopItem, IShopImportSettings, ILogger> _loggerInterception = loggerInterception;

    ILogger<T> IImportServiceLogFactory.GetLogger<T>(ILogger<T> logger, IShopItem shopModel, IShopImportSettings shopImportSettings)
    {
        var interceptedLogger = _loggerInterception(logger, shopModel, shopImportSettings);
        return new LogEmptyInterceptorImpl<T>(interceptedLogger);
    }
}
