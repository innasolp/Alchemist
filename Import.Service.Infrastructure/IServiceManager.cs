using Import.Interfaces;
using Import.Settings.Interfaces;

namespace Import.Service.Infrastructure;

public interface IServiceManager
{
    Task<(Guid guid, IImportService service)> AddImportService(string name, IImportSettings importSettings,
        CancellationToken cancellationToken = default);

    (bool connecting, IImportService service, Task? startTask) StartService(Guid guid, CancellationToken cancellationToken);

    IEnumerable<(Guid guid, IImportService service, Task stopTask)> StopAllServicesTask(CancellationToken cancellationToken = default);

    (IImportService service, Task startTask) StopServiceTask(Guid guid, CancellationToken cancellationToken);
}
