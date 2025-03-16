using Microsoft.Extensions.Hosting;

namespace Alchemist.Import.Settings.Builders;

public interface ISettingsBuilder
{
    Task<List<ShopSettingsContainer>> Build(IHost host);
}
