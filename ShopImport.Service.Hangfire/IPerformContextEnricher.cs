using Hangfire.Server;
using ShopImport.Service.Hangfire.Infrastructure;

namespace ShopImport.Service.Hangfire;

internal interface IPerformContextEnricher
{
    void Enrich(PerformContext performContext, IImportServiceJob importServiceJob, IReadOnlyDictionary<Guid, IImportServiceJob> allImportServiceJobs);
}