using WebLoader.Interfaces;

namespace Alchemist.Import.Interfaces;

public interface IImportService
{
    Task Start(CancellationToken stoppingToken);

    string Name { get; }

    IWebLoader WebLoader { get; }
}
