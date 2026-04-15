using Alchemist.Import.Settings.Extensions;
using Hangfire.AggregateJobs;
using Import.Factory.Interfaces;
using Import.Service;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal class ShopImportServiceJobFactory(ILoggerFactory loggerFactory, 
    IImportServiceLogFactory importServiceLogFactory) 
    : IImportServiceJobFactory
{
    private readonly ILoggerFactory _loggerFactory = loggerFactory;

    private readonly IImportServiceLogFactory _importServiceLogFactory = importServiceLogFactory;

    public IImportServiceJob CreateServiceJob<TImportSource, TImportSourceItem>(IImportServiceFactory importServiceFactory, 
        IImportSettings importSettings,
        string name, 
        TImportSource source,
        bool isAggregate = false,
        Guid? parentId = null)
         where TImportSource :
        IImportSource,
        ISplittableSource<TImportSource>, 
        IIdentificableSource, 
        ISourceItemCollection<TImportSource,
            TImportSourceItem>
    {
        var serviceExecutionOptions = importSettings.GetService(nameof(JobExecuteOptions))?.GetServiceValue<JobExecuteOptions>();

        var obj = new { Name = name, SourceName = source.Name, SourceUrl = source.Url };
        var id = GuidGenerator.ToDeterministicGuid(obj);

        if (!isAggregate)
        {
            var importService = importServiceFactory.Create(name, source, importSettings);
            return new SingleImportServiceJob<TImportSource>(importService, source, id, parentId, serviceExecutionOptions);
        }
        else
        {
            var logger = _loggerFactory.CreateLogger<AggregateService>();
            var serviceLogger = _importServiceLogFactory.GetLogger(logger, name, source, importSettings);
            return new AggregateImportServiceJob<TImportSource, TImportSourceItem>(this,
                serviceLogger, 
                name,
                source,
                importSettings, 
                importServiceFactory,
                id,
                serviceExecutionOptions);
        }
    }
}