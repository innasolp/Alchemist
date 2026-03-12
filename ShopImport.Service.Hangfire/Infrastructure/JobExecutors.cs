using Alchemist.Import.Settings;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal static class JobExecutors
{
    public static IJobExecutor GetJobExecutor(IShopImportSettings shopImportSettings, bool child = false)
    {
        throw new NotImplementedException();
    }
}