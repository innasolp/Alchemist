using Hangfire;

namespace ShopImport.Service.Hangfire.Infrastructure.JobExecutors;

internal class HangfireTokenAdapter(CancellationToken token) : IJobCancellationToken
{
    private readonly CancellationToken _token = token;

    public void ThrowIfCancellationRequested() => _token.ThrowIfCancellationRequested();

    public CancellationToken ShutdownToken => _token;
}