using Hangfire.Server;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal interface IPerformContextEnricher
{
    void Enrich(PerformContext performContext, IImportServiceJob importServiceJob, IReadOnlyDictionary<Guid, IImportServiceJob> allImportServiceJobs);
}