using Alchemist.Import.Settings.Interfaces;
using System.Collections;
using System.Text.Json.Serialization;

namespace Alchemist.Import.Settings.Test.Model;

public abstract class TestShopImportSettings : IShopImportSettings
{
    public bool? Perfomance { get; set; }

    [JsonIgnore]
    public int Id { get; set; }

    public string Name { get; set; }

    [JsonIgnore]
    public int ShopId { get; set; }

    public List<TestImportServiceSettings> Services { get; set; } = [];    

    IList IShopImportSettings.Services => Services;

    int? ISettings.ParentSettingsId
    {
        get { return null; }
        set {; }
    }

    protected abstract ShopSettingType ShopSettingType { get; }

    ShopSettingType ISettings.ShopSettingType => ShopSettingType;

    public string ShopName { get; set; }
    public string ShopUrl { get; set; }
}
