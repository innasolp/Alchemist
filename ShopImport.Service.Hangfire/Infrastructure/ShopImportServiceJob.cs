using Alchemist.Import.Settings;
using Import.Factory.Interfaces;
using Import.Interfaces;
using ShopImport.Service.Hangfire.Models;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal class ShopImportServiceJob(string name,
    IShopModel shopModel,
    IShopImportSettings shopImportSettings,
    IImportServiceFactory importServiceFactory,
    Guid? parentId = null) : IImportServiceJob
{
    public IImportService ImportService { get; } = importServiceFactory.Create(name, shopModel, shopImportSettings);

    public int SourceId { get; } = shopModel.Id;

    public Guid Id { get; } = Guid.NewGuid();

    public string? JobId { get; set; }

    public Guid? ParentId { get; } = parentId;

    Task<IDictionary<Guid, IImportServiceJob>> IImportServiceJob.GetExecutionServiceJobs()
    {
        IDictionary<Guid, IImportServiceJob> result = new Dictionary<Guid, IImportServiceJob>() { { Id, this } };
        return Task.FromResult(result);
    }
}