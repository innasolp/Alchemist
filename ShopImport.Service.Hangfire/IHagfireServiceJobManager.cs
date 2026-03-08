using Hangfire;
using Hangfire.Server;
using System.ComponentModel;

namespace ShopImport.Service.Hangfire;

public interface IHagfireServiceJobManager
{
    [DisplayName("{1}")]
    [AutomaticRetry(Attempts =0)]
    Task Execute(Guid id, string displayName, CancellationToken cancellationToken = default, PerformContext? performContext = null);
}