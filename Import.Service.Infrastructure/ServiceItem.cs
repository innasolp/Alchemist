using Import.Interfaces;

namespace Import.Service.Infrastructure;

public class ServiceItem(Guid guid, IImportService service, int sourceId, CancellationTokenSource innerTokenSource)
{
    public Guid Guid { get; } = guid;

    public IImportService Service { get; } = service;

    public int SourceId { get; } = sourceId;

    public CancellationTokenSource InnerTokenSource { get; } = innerTokenSource;
}