namespace ShopImport.Service.Hangfire.Infrastructure.JobManagement.JobExecutors;

internal static class ImportServiceJobExtensions
{
    public static async Task<IReadOnlyDictionary<Guid,IImportServiceJob>> GetСhildJobs(this IImportServiceJob importServiceJob)
    {
        return (await importServiceJob.GetExecutionServiceJobs()).Where(s=>s.Value.ParentId == importServiceJob.Id).ToDictionary();    
    }
}