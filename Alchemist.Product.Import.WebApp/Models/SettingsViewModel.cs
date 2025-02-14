using Alchemist.Product.Import.WebApp.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace Alchemist.Product.Import.WebApp.Models;

public class SettingsViewModel(ShopModel shop) : ITabViewModel
{
    [Required]
    public ShopModel Shop { get; set; } = shop;

    public bool IsActive { get; set; }

    public string PartialViewName => "Settings";

    public string[] SettingsTabs => ["Product", "Category"];
}
