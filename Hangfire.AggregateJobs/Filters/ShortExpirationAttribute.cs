using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace Hangfire.AggregateJobs.Filters;

public class ShortExpirationAttribute(int minutes = 1) : JobFilterAttribute, IApplyStateFilter
{
    private readonly int _minutes = minutes;

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.NewState is SucceededState)        
            context.JobExpirationTimeout = TimeSpan.FromMinutes(_minutes);        
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
    }
}