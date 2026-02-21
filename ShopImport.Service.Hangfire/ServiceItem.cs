using Import.Interfaces;

namespace ShopImport.Service.Hangfire;

public class ServiceItem(Guid guid, IImportService service, int sourceId, CancellationTokenSource innerTokenSource)
{
    public Guid Guid { get; } = guid;

    public IImportService Service { get; } = service;

    public int SourceId { get; } = sourceId;

    public string? JobId { get; set; }
}