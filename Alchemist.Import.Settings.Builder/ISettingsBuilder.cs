using Alchemist.Import.Settings.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Alchemist.Import.Settings.Builders;

public interface ISettingsBuilder
{
    Task<List<IShopImportData>> Build(IHost host);
}
