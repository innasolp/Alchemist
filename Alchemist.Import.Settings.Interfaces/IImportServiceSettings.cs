using DependencyInjection.Interfaces;
using System.Text.Json.Nodes;

namespace Alchemist.Import.Settings.Interfaces;

public interface IImportServiceSettings : IServiceSettings, ISettings
{
    Guid Guid { get; set; }
}
