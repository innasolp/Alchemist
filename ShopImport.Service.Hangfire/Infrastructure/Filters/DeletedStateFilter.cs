using Hangfire.States;

namespace ShopImport.Service.Hangfire.Infrastructure.Filters;

internal class DeletedStateFilter : IElectStateFilter
{
    public void OnStateElection(ElectStateContext context)
    {
        if (context.CandidateState is not FailedState failedState 
            || failedState.Exception is not JobDeletedException jobDeletedException)
            return;

        context.CandidateState = new DeletedState();
    }
}