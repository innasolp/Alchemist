using Hangfire.AggregateJobs;
using Hangfire.Tags;

namespace ShopImport.Service.Hangfire.Infrastructure.Enrichers;

internal class ParentJobTagEnricher : IChildJobEnricher<IImportServiceJob>
{
    public void Enrich(string jobId, IImportServiceJob job, IImportServiceJob parentJob)
    {
        jobId.AddTags(parentJob.ImportService.Name);
    }
}