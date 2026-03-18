using Hangfire.Server;
using Hangfire.Tags;

namespace ShopImport.Service.Hangfire.Infrastructure.PerformContextEnrichers;

internal class ParentTagEnricher : IPerformContextEnricher
{
    public void Enrich(PerformContext performContext, IImportServiceJob importServiceJob, IReadOnlyDictionary<Guid, IImportServiceJob> allImportServiceJobs)
    {
        var parentServiceJob = importServiceJob.ParentId.HasValue
               && allImportServiceJobs.TryGetValue(importServiceJob.ParentId.Value, out var parentJob)
               && parentJob != null
                ? parentJob
                : importServiceJob;
        performContext.AddTags(parentServiceJob.ImportService.Name);
    }
}