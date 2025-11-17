using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;
using System.Collections;
using System.Text.Json.Serialization;

namespace Alchemist.Import.Settings.Test.Model;

public abstract class TestShopImportSettings : IShopImportSettings, IShopSettings
{
    public bool? Perfomance { get; set; }

    [JsonIgnore]
    public int Id { get; set; }

    public string Name { get; set; }

    [JsonIgnore]
    public int ShopId { get; set; }

    public Dictionary<string, TestImportServiceSettings> Services { get; set; } = [];    

    IDictionary IShopImportSettings.Services => Services;

    int? IShopSettings.ParentSettingsId
    {
        get { return null; }
        set {; }
    }

    public abstract ShopSettingType Type { get; }

    public string ShopName { get; set; }
    public string ShopUrl { get; set; }
    bool? IShopSettings.IsActual { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    string IShopSettings.JsonValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    ShopSettingType IShopSettings.Type { get => Type; set {; } }
}
