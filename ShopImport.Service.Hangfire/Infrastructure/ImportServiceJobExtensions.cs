namespace ShopImport.Service.Hangfire.Infrastructure;

internal static class ImportServiceJobExtensions
{
    public static async Task<IEnumerable<IImportServiceJob>> GetСhildJobs(this IImportServiceJob importServiceJob)
    {
        return (await importServiceJob.GetExecutionServiceJobs()).Where(s=>s.Value.ParentId == importServiceJob.Id).Select(s=>s.Value);    
    }

    public static async Task<IEnumerable<string?>> GetСhildJobIds(this IImportServiceJob importServiceJob)
    {
        return (await importServiceJob.GetExecutionServiceJobs()).Where(s=>s.Value.ParentId == importServiceJob.Id).Select(s=>s.Value.JobId);    
    }
}