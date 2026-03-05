using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Service;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;
using ShopImport.Service.Hangfire.Models;
using ShopImport.Service.Infrastructure.Module.Models;
using System.Collections.Concurrent;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal class AggregateShopImportServiceJob : IImportServiceJob
{
    private readonly IImportServiceFactory _importServiceFactory;

    private readonly IShopImportSettings _shopImportSettings;

    private readonly IShopModel _shopModel;

    private readonly string _name;

    private readonly ConcurrentQueue<(IProductShopCategory, IImportService)> _categoryImportServices = [];

    private readonly Dictionary<Guid, IImportServiceJob> _importServiceJobs = [];

    public AggregateShopImportServiceJob(ILogger logger,
        string name, 
        IShopModel shopModel,
        IShopImportSettings shopImportSettings,
        IImportServiceFactory importServiceFactory)
    {
        _name = name;
        _shopModel = shopModel;
        _importServiceFactory = importServiceFactory;
        _shopImportSettings = shopImportSettings;
        SourceId = _shopModel.Id;
        ImportService = new AggregateImportService(logger, name, OnExecuteService);
        InitializeImportServiceJobs();
    }

    private void InitializeImportServiceJobs()
    {
        foreach (var productShopCategory in _shopModel.RootCategories)
        {
            var serviceName = $"{_name}/{productShopCategory.Category}";
            var currentShopModel = new ProductShopModel {
                Name = (_shopModel as IImportSource).Name,
                Caption = _shopModel.Caption,
                Id = _shopModel.Id,
                Url = (_shopModel as IImportSource).Url
            };
            currentShopModel.RootCategories.Add(productShopCategory);
            var importServiceJob = new ShopImportServiceJob(serviceName, currentShopModel, _shopImportSettings, _importServiceFactory);
            _importServiceJobs.Add(importServiceJob.Guid, importServiceJob);
            _categoryImportServices.Enqueue((productShopCategory, importServiceJob.ImportService));
        }
    }

    public IAggregateImportService ImportService { get; }

    public int SourceId { get; }

    public Guid Guid { get; } = Guid.NewGuid();

    public string? JobId { get; set; }

    IImportService IImportServiceJob.ImportService => ImportService;

    private async Task OnExecuteService(IImportService importService, CancellationToken cancellationToken)
    {
        //todo
    }

    public Task<IDictionary<Guid, IImportServiceJob>> GetExecutionServiceJobs()
    {
        //todo 
        IDictionary<Guid, IImportServiceJob> result = new Dictionary<Guid, IImportServiceJob>
        {
            { Guid, this }
        };

        foreach(var serviceJob in  _importServiceJobs) 
            result.Add(serviceJob.Key, serviceJob.Value);

        return Task.FromResult(result);
    }
}