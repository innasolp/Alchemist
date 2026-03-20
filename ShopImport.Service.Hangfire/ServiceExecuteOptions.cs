namespace ShopImport.Service.Hangfire;

internal class ServiceExecuteOptions
{
    public int? EnqueuedInSeconds { get; set; } = null;

    public int? IntervalInSeconds { get; set; } = null;
}
