using DependencyInjection.Interfaces;

namespace Alchemist.Import.Settings.Interfaces;

public interface IImportServiceSettings : IServiceSettings
{
    int? Id { get; set; }

    string? Name { get; set; }

    int? ParentSettingsId { get; set; }
}
