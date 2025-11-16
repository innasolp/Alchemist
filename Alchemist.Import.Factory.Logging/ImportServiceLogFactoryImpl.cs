using Alchemist.Import.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Factory.Logging;

internal class ImportServiceLogFactoryImpl(Func<ILogger, string, IShopItem, IShopImportSettings, ILogger> loggerInterception) : IImportServiceLogFactory
{
    private readonly Func<ILogger, string, IShopItem, IShopImportSettings, ILogger> _loggerInterception = loggerInterception;    

    ILogger<T> IImportServiceLogFactory.GetLogger<T>(ILogger<T> logger, string name, IShopItem shopModel, IShopImportSettings shopImportSettings)
    {
        var interceptedLogger = _loggerInterception(logger, name, shopModel, shopImportSettings);
        return new LogEmptyInterceptorImpl<T>(interceptedLogger);
    }
}
