using Alchemist.Import.Settings;
using Import.Factory.Interfaces;
using Import.Interfaces;
using ShopImport.Service.Infrastructure.Module.Models;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal class ShopImportServiceJob(string name,
    IShopModel shopModel,
    IShopImportSettings shopImportSettings,
    IImportServiceFactory importServiceFactory) : IImportServiceJob
{
    public IImportService ImportService { get; } = importServiceFactory.Create(name, shopModel, shopImportSettings);

    public int SourceId { get; } = shopModel.Id;

    public Guid Guid { get; } = Guid.NewGuid();

    public string? JobId { get; set; }

    Task<IDictionary<Guid, IImportServiceJob>> IImportServiceJob.GetExecutionServiceJobs()
    {
        IDictionary<Guid, IImportServiceJob> result = new Dictionary<Guid, IImportServiceJob>() { { Guid, this } };
        return Task.FromResult(result);
    }
}