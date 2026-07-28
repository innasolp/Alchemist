using Hangfire.AggregateJobs;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Service;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal class AggregateImportServiceJob<TImportSource, TSourceItem> : ImportServiceJob, ISourceItemListenerJob<TSourceItem>, IDisposable
    where TImportSource : IImportSource,
    ISplittableSource<TImportSource>, 
    IIdentificableSource,
    ISourceItemCollection<TImportSource, TSourceItem>
{
    private readonly IImportServiceFactory _importServiceFactory;

    private readonly IImportSettings _importSettings;

    private readonly TImportSource _importSource;

    private readonly IImportServiceJobFactory _importServiceJobFactory;

    private readonly string _name;

    private readonly Dictionary<Guid, IImportServiceJob> _importServiceJobs = [];

    private readonly IAggregateImportService _aggregateImportService;

    public override IImportService ImportService => _aggregateImportService;

    public override int SourceId => _importSource.Id;

    protected override bool IsAggregate => true;

    public AggregateImportServiceJob(IImportServiceJobFactory importServiceJobFactory,
        ILogger logger,
        string name,
        TImportSource importSource,
        IImportSettings importSettings,
        IImportServiceFactory importServiceFactory, 
        Guid id,
        JobExecuteOptions? jobExecuteOptions = null)
        : base(id, null, jobExecuteOptions)
    {
        _importServiceJobFactory = importServiceJobFactory;
        _name = name;
        _importSource = importSource;
        _importServiceFactory = importServiceFactory;
        _importSettings = importSettings;
        _aggregateImportService = new AggregateService(logger, name, OnExecuteService);
        _aggregateImportService.ConnectedAsync += AggregateImportServiceConnectedAsync;   
    }

    private async Task AggregateImportServiceConnectedAsync(object sender, ConnectedAsyncEventArgs eventArgs)
    {
        if (sender is not IAggregateImportService aggregateImportService) return;

        if (!eventArgs.Connected)
        {
            var children = _importServiceJobs.ToDictionary();
            foreach (var childJob in children)
                await RemoveChildJob(childJob.Value);   
        }
    }

    private void InitializeImportServiceJobs()
    {
        var executionSources = _importSource.Split();

        foreach (var source in executionSources)        
            AddNewImportServiceJob(source);        
    }

    private async Task ChildServiceConnectedAsync(object sender, ConnectedAsyncEventArgs eventArgs)
    {
        if (sender is not IImportService importService) return;

        if (!eventArgs.Connected || eventArgs.CancellationToken.IsCancellationRequested)
        {
            var serviceJob = _importServiceJobs.FirstOrDefault(x => x.Value.ImportService.Name == importService.Name);
            if (serviceJob.Value == null) return;

            await RemoveChildJob(serviceJob.Value);
        }
    }

    private async Task RemoveChildJob(IImportServiceJob childJob)
    {
        await _aggregateImportService.TryRemove(childJob.ImportService);

        childJob.ImportService.ConnectedAsync -= ChildServiceConnectedAsync;

        _importServiceJobs.Remove(childJob.Id);
    }

    private async Task OnExecuteService(IImportService importService, CancellationToken cancellationToken)
    {
        //todo
    }

    protected override Task<IReadOnlyDictionary<Guid, IImportServiceJob>> GetExecutionServiceJobs()
    {
        if (_importServiceJobs.Count == 0)
            InitializeImportServiceJobs();

        IDictionary<Guid, IImportServiceJob> result = new Dictionary<Guid, IImportServiceJob>
        {
            { Id, this }
        };

        foreach (var serviceJob in _importServiceJobs)
            result.Add(serviceJob.Key, serviceJob.Value);

        return Task.FromResult((IReadOnlyDictionary<Guid, IImportServiceJob>)result);
    }

    public void Dispose()
    {
        _aggregateImportService.ConnectedAsync -= AggregateImportServiceConnectedAsync;
        foreach (var childJob in _importServiceJobs)
            childJob.Value.ImportService.ConnectedAsync -= ChildServiceConnectedAsync;
    }

    IImportServiceJob ISourceItemListenerJob<TSourceItem>.AddSource(TSourceItem message)
    {
        var source = _importSource.AddSourceItem(message);
        return AddNewImportServiceJob(source);
    }

    private IImportServiceJob AddNewImportServiceJob(TImportSource source)
    {
        var serviceName = $"{_name}/{source.Name}";
        var importServiceJob = _importServiceJobFactory.CreateServiceJob<TImportSource, TSourceItem>(
                    _importServiceFactory, _importSettings, serviceName, source, false, Id);
        
        _importServiceJobs.Add(importServiceJob.Id, importServiceJob);

        _aggregateImportService.Enqueue(importServiceJob.ImportService);

        importServiceJob.ImportService.ConnectedAsync += ChildServiceConnectedAsync;

        return importServiceJob;
    }
}