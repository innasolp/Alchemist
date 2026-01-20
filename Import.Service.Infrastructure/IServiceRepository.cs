using Import.Interfaces;
using Import.Settings.Interfaces;

namespace Import.Service.Commands;

public interface IServiceRepository
{
    IReadOnlyDictionary<Guid, ServiceItem> Services { get; }

    IReadOnlyList<IImportSource> ShopModels { get; }

    Task<(bool, Guid guid, IImportService? service)> TryAddImportService(string name, IImportSettings importSettings, 
        CancellationToken cancellationToken = default);
}
