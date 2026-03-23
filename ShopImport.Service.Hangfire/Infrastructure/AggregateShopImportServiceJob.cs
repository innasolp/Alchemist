using Alchemist.Import.Settings;
using Hangfire.AggregateJobs;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Service;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;
using ShopImport.Service.Hangfire.Models;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal class AggregateShopImportServiceJob : ImportServiceJob
{
    private readonly IImportServiceFactory _importServiceFactory;

    private readonly IShopImportSettings _shopImportSettings;

    private readonly IShopModel _shopModel;

    private readonly IImportServiceJobFactory _shopImportServiceJobFactory;

    private readonly string _name;

    private readonly Dictionary<Guid, IImportServiceJob> _importServiceJobs = [];

    private readonly IAggregateImportService _aggregateImportService;

    public override IImportService ImportService => _aggregateImportService;

    public override int SourceId => _shopModel.Id;

    public AggregateShopImportServiceJob(IImportServiceJobFactory shopImportServiceJobFactory,
        ILogger logger,
        string name,
        IShopModel shopModel,
        IShopImportSettings shopImportSettings,
        IImportServiceFactory importServiceFactory, 
        JobExecuteOptions? jobExecuteOptions = null)
        : base(null, jobExecuteOptions)
    {
        _shopImportServiceJobFactory = shopImportServiceJobFactory;
        _name = name;
        _shopModel = shopModel;
        _importServiceFactory = importServiceFactory;
        _shopImportSettings = shopImportSettings;
        _aggregateImportService = new AggregateService(logger, name, OnExecuteService);
        _aggregateImportService.ConnectedAsync += AggregateImportServiceConnectedAsync;   
    }

    private async Task AggregateImportServiceConnectedAsync(object sender, ConnectedAsyncEventArgs eventArgs)
    {
        if (sender is not IAggregateImportService aggregateImportService) return;

        if (!eventArgs.Connected || eventArgs.CancellationToken.IsCancellationRequested)
        {
            var children = _importServiceJobs.ToDictionary();
            foreach (var childJob in children)
            {
                await aggregateImportService.TryRemove(childJob.Value.ImportService);
                childJob.Value.ImportService.ConnectedAsync -= ChildServiceConnectedAsync;
            }

            _importServiceJobs.Clear();
        }
    }

    private void InitializeImportServiceJobs()
    {
        var executionSources = _shopModel.Split();

        foreach (var source in executionSources)
        {
            var serviceName = $"{_name}/{(source as IImportSource).Name}";
            var importServiceJob = _shopImportServiceJobFactory.CreateServiceJob(_importServiceFactory, _shopImportSettings, serviceName, _shopModel, false, Id);
            _importServiceJobs.Add(importServiceJob.Id, importServiceJob);

            _aggregateImportService.Enqueue(importServiceJob.ImportService);

            importServiceJob.ImportService.ConnectedAsync += ChildServiceConnectedAsync;
        }
    }

    private async Task ChildServiceConnectedAsync(object sender, ConnectedAsyncEventArgs eventArgs)
    {
        if (sender is not IImportService importService) return;

        if (!eventArgs.Connected || eventArgs.CancellationToken.IsCancellationRequested)
        {
            var serviceJob = _importServiceJobs.FirstOrDefault(x => x.Value.ImportService.Name == importService.Name);
            if (serviceJob.Value == null) return;

            await _aggregateImportService.TryRemove(serviceJob.Value.ImportService);
            serviceJob.Value.ImportService.ConnectedAsync -= ChildServiceConnectedAsync;

            _importServiceJobs.Remove(serviceJob.Key);
        }
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
}