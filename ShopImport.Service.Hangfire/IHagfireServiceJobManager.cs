using Hangfire;
using Hangfire.Server;
using System.ComponentModel;

namespace ShopImport.Service.Hangfire;

public interface IHagfireServiceJobManager
{
    [DisplayName("{1}")]
    [AutomaticRetry(Attempts =0)]
    Task Execute(Guid id,
        string displayName, 
        bool isAggregate = false,
        Guid? parentId = null,
        IJobCancellationToken? jobCancellationToken = null,
        CancellationToken cancellationToken = default,
        PerformContext? performContext = null);
}