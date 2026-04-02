using Hangfire.AggregateJobs;
using Import.Interfaces;
using Import.Settings.Interfaces;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal class SingleImportServiceJob<TImportSource>(IImportService importService,
    TImportSource importSource,
    Guid? parentId = null,
    JobExecuteOptions? jobExecuteOptions = null) : ImportServiceJob(parentId, jobExecuteOptions)
    where TImportSource : IImportSource, IIdentificableSource
{
    private readonly TImportSource _importSource = importSource;

    public override IImportService ImportService { get; } = importService;

    public override int SourceId => _importSource.Id;

    protected override bool IsAggregate => false;
}