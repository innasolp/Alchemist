using Hangfire.Common;
using Hangfire.States;

namespace ShopImport.Service.Hangfire.Infrastructure.JobManagement.JobExecutors.Filter;

internal class ChangeQueueFilter : JobFilterAttribute, IElectStateFilter
{
    private const string ProcessingQueueParameter = "ProcessingQueue";

    public void OnStateElection(ElectStateContext context)
    {
        if (context.CurrentState?.Equals("processing", StringComparison.InvariantCultureIgnoreCase) != true
            && context.CandidateState is ProcessingState)
        {
            var processingQueue = context.Connection.GetJobParameter(context.BackgroundJob.Id, ProcessingQueueParameter);

            if (string.IsNullOrEmpty(processingQueue)) return;

            context.Connection.SetJobParameter(context.BackgroundJob.Id, "Queue", processingQueue);
        }
    }
}