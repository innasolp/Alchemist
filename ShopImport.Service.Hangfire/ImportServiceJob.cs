using Import.Interfaces;
using LongRunningTask;

namespace ShopImport.Service.Hangfire;

internal class ImportServiceJob(ServiceItem importServiceItem) : IJobService
{
    private readonly ServiceItem _importServiceItem = importServiceItem;

    public Task Execute(CancellationToken cancellationToken = default)
    {
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        return StartAndDisposeAsync(_importServiceItem.Service,linkedCts);
    }

    static async Task StartAndDisposeAsync(IImportService service, CancellationTokenSource linkedCts)
    {
        try
        {
            await service.Start(linkedCts.Token);
        }
        finally
        {
            linkedCts.Dispose();
        }
    }
    
    public Task Resume(CancellationToken cancellationToken = default)
    {
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        return StartAndDisposeAsync(_importServiceItem.Service, linkedCts);
    }

    public Task Pause(CancellationToken cancellationToken = default)
    {
        return _importServiceItem.Service.Stop(cancellationToken);
    }

    public Task Stop(CancellationToken cancellationToken = default)
    {
        return _importServiceItem.Service.Stop(cancellationToken);
    }
}