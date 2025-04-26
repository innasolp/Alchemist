using DependencyInjection.Interfaces;

namespace Alchemist.Import.Settings.Interfaces;

public interface IImportServiceSettings : IServiceSettings, ISettings
{
    Guid Guid { get; set; }
}
