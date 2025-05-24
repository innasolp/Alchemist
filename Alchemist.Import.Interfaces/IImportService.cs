namespace Alchemist.Import.Interfaces;

public interface IImportService
{
    Task Start(CancellationToken stoppingToken);

    string Name { get; }
}
