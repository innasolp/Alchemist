using Import.Factory.Interfaces;
using Import.Settings.Interfaces;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal interface IImportServiceJobFactory
{
    IImportServiceJob CreateServiceJob(IImportServiceFactory importServiceFactory, IImportSettings shopImportSettings, string name, IImportSource source, Guid? parentId = null);
}