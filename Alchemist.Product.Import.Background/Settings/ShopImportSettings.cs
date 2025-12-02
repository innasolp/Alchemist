using Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;
using System.Collections;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.Background.Settings;

public abstract class ShopImportSettings : IShopImportSettings, IShopSettings
{
    public bool? Perfomance { get; set; }

    [JsonIgnore]
    public int Id { get; set; }

    public string Name { get; set; }

    [JsonIgnore]
    public int ShopId { get; set; }
    
    public Dictionary<string,ImportServiceSettings> Services { get; set; } = [];

    IDictionary IShopImportSettings.Services => Services;

    int? IShopSettings.ParentSettingsId 
    { 
        get { return null; }
        set {; }
    }

    public abstract ShopSettingType ShopSettingType { get; }   

    public string ShopName { get; set; }
    public string ShopUrl { get; set; }
    bool? IShopSettings.IsActual { get ; set; }
    string IShopSettings.JsonValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    ShopSettingType IShopSettings.Type { get => ShopSettingType; set => throw new InvalidOperationException(); }
}
