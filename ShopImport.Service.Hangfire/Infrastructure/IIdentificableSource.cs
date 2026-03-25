namespace ShopImport.Service.Hangfire.Infrastructure;

internal interface IIdentificableSource
{
    int Id { get; }
}