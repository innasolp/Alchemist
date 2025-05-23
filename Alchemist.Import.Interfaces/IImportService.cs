namespace Alchemist.Import.Interfaces;

public interface IImportService
{
    Task Start(CancellationTokenSource stoppingToken);

    string Name { get; }
}
