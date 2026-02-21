namespace LongRunningTask;

public interface IJobService
{
    Task Execute(CancellationToken cancellationToken = default);

    Task Pause(CancellationToken cancellationToken = default);

    Task Resume(CancellationToken cancellationToken = default);

    Task Stop(CancellationToken cancellationToken = default);
}
