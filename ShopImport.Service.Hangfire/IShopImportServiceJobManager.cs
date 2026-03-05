using Import.Service.Infrastructure;

namespace ShopImport.Service.Hangfire;

public interface IShopImportServiceJobManager : IShopImportServiceManager, IImportServiceJobManager
{
}