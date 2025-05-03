namespace Alchemist.Import.Settings.Builders;

public interface ISettingsBuilder
{
    Task<List<ShopSettingsContainer>> Build();

    int Priority { get; }
}
