namespace ShopImport.Service.Hangfire.Infrastructure;

internal interface ISourceItemCollection<Tsource, TSourceItem>    
{
    Tsource AddSourceItem(TSourceItem item);
}