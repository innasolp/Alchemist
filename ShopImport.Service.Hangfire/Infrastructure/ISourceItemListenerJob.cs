namespace ShopImport.Service.Hangfire.Infrastructure;

internal interface ISourceItemListenerJob<TSourceItem>
{
    IImportServiceJob AddSource(TSourceItem item);
}