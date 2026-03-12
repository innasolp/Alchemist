using Alchemist.Import.Settings;
using Hangfire;
using Import.Factory.Interfaces;
using Import.Service;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;
using ShopImport.Service.Hangfire.Models;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal class ShopImportServiceJobFactory(IBackgroundJobClient backgroundJobClient, ILoggerFactory loggerFactory, IImportServiceLogFactory importServiceLogFactory) 
    : IImportServiceJobFactory
{
    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    private readonly ILoggerFactory _loggerFactory = loggerFactory;

    private readonly IImportServiceLogFactory _importServiceLogFactory = importServiceLogFactory;

    public IImportServiceJob CreateServiceJob(IImportServiceFactory importServiceFactory, IImportSettings importSettings, string name, IImportSource source, Guid? parentId = null)
    {
        if (importSettings is not IShopImportSettings shopImportSettings)
            throw new InvalidOperationException($"Invalid import settings type {importSettings.GetType().Name}. Must be assignable from {nameof(IShopImportSettings)}.");

        if(source is not IShopModel shopModel)
            throw new InvalidOperationException($"Invalid source type {source.GetType().Name}. Must be assignable from {nameof(IShopModel)}.");

        if (!shopImportSettings.IsAggregate)
        {
            var jobExecutor = JobExecutors.GetJobExecutor(shopImportSettings, parentId != null);
            var importService = importServiceFactory.Create(name, source, shopImportSettings);
            return new SingleShopImportServiceJob(importService, jobExecutor, _backgroundJobClient, shopModel, parentId);
        }
        else
        {
            var jobExecutor = JobExecutors.GetJobExecutor(shopImportSettings);
            var logger = _loggerFactory.CreateLogger<AggregateImportService>();
            var serviceLogger = _importServiceLogFactory.GetLogger(logger, name, source, shopImportSettings);
            return new AggregateShopImportServiceJob(jobExecutor, 
                _backgroundJobClient, 
                this,
                serviceLogger, 
                name, 
                shopModel, 
                shopImportSettings, 
                importServiceFactory);
        }
    }
}