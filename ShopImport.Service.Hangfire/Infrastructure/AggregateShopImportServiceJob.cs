using Alchemist.Import.Settings;
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
        IImportServiceFactory importServiceFactory)
        : base(null)
    {
        _shopImportServiceJobFactory = shopImportServiceJobFactory;
        _name = name;
        _shopModel = shopModel;
        _importServiceFactory = importServiceFactory;
        _shopImportSettings = shopImportSettings;
        _aggregateImportService = new AggregateImportService(logger, name, OnExecuteService);
        InitializeImportServiceJobs();
    }

    private void InitializeImportServiceJobs()
    {
        var executionSources = _shopModel.Split();

        foreach (var source in executionSources)
        {
            var serviceName = $"{_name}/{(source as IImportSource).Name}";
            var importServiceJob = _shopImportServiceJobFactory.CreateServiceJob(_importServiceFactory, _shopImportSettings, serviceName, _shopModel, false, Id);
            _importServiceJobs.Add(importServiceJob.Id, importServiceJob);
        }
    }

    private async Task OnExecuteService(IImportService importService, CancellationToken cancellationToken)
    {
        //todo
    }

    protected override Task<IDictionary<Guid, IImportServiceJob>> GetExecutionServiceJobs()
    {
        IDictionary<Guid, IImportServiceJob> result = new Dictionary<Guid, IImportServiceJob>
        {
            { Id, this }
        };

        foreach(var serviceJob in  _importServiceJobs) 
            result.Add(serviceJob.Key, serviceJob.Value);

        return Task.FromResult(result);
    }
}