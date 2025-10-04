using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.Test.Model;
using Alchemist.Product.Interfaces;

namespace Alchemist.Import.Settings.DataAdapter.Tests;

internal static class AssertService
{
    internal static void Exists(IEnumerable<IShopSettings> sourceServices, IShopImportSettings shopImportSettings, string serviceName)
    {
        if (sourceServices.Any(s => s.Name == serviceName))
            Assert.NotNull(shopImportSettings.GetService<TestImportServiceSettings>(serviceName));
        else
            Assert.Null(shopImportSettings.GetService<TestImportServiceSettings>(serviceName));
    }

    internal static void PrimaryServicesExist(IEnumerable<IShopSettings> sourceServices, IShopImportSettings shopImportSettings)
    {
        Exists(sourceServices, shopImportSettings, nameof(PrimaryServiceName.ImportService));
        Exists(sourceServices, shopImportSettings, nameof(PrimaryServiceName.RequestHeaders));
        Exists(sourceServices, shopImportSettings, nameof(PrimaryServiceName.BrowserDataLoader));
        Exists(sourceServices, shopImportSettings, nameof(PrimaryServiceName.WebLoader));
        Exists(sourceServices, shopImportSettings, nameof(PrimaryServiceName.BrowserLauncher));
    }
}
