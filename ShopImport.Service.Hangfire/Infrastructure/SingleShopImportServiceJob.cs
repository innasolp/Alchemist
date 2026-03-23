using Hangfire.AggregateJobs;
using Import.Interfaces;
using ShopImport.Service.Hangfire.Models;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal class SingleShopImportServiceJob(IImportService importService,
    IShopModel shopModel,
    Guid? parentId = null,
    JobExecuteOptions? jobExecuteOptions = null) : ImportServiceJob(parentId, jobExecuteOptions)
{
    private readonly IShopModel shopModel = shopModel;

    public override IImportService ImportService { get; } = importService;

    public override int SourceId => shopModel.Id;
}