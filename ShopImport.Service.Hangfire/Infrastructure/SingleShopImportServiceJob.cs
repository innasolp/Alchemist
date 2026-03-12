using Hangfire;
using Import.Interfaces;
using ShopImport.Service.Hangfire.Models;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal class SingleShopImportServiceJob(IImportService importService,
    IJobExecutor jobExecutor, 
    IBackgroundJobClient backgroundJobClient, 
    IShopModel shopModel, 
    Guid? parentId = null)
        : ImportServiceJob(jobExecutor, backgroundJobClient, parentId)
{
    public override IImportService ImportService { get; } = importService;

    public override int SourceId => shopModel.Id;
}