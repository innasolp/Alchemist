using Alchemist.Import.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Logging.Factory;

public static class FactoryLoggingDependencyInjectionExtensions
{
    public static IServiceCollection AddImportServiceLogFactory(this IServiceCollection services, Func<ILogger, IShopItem, IShopImportSettings, ILogger> getLoggerForShop)
    {
        return services.AddSingleton<IImportServiceLogFactory>(new ImportServiceLogFactoryImpl(getLoggerForShop));
    }
}
