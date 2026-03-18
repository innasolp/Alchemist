using Hangfire;
using Hangfire.Server;
using ShopImport.Service.Hangfire.Infrastructure.Filters;
using System.ComponentModel;

namespace ShopImport.Service.Hangfire;

public interface IHagfireServiceJobManager
{
    [DisplayName("{1}")]
    [AutomaticRetry(Attempts =0)]
    [ChangeQueueFilter]
    Task Execute(Guid id, string displayName, bool isAggregate = false, Guid? parentId = null, CancellationToken cancellationToken = default, PerformContext? performContext = null);
}