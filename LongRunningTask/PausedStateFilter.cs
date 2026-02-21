using Hangfire.States;
using Hangfire.Storage;

namespace LongRunningTask;

public class PausedStateFilter : IApplyStateFilter
{
    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if(context.NewState is PausedState pausedState)
        {
            //todo write state to db or send message
            //context.Storage.
            
            pausedState.Reason = $"Job is paused at {DateTime.UtcNow}";
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
       //todo
    }
}