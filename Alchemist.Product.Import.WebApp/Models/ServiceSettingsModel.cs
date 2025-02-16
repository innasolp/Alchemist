using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace Alchemist.Product.Import.WebApp.Models;

public class ServiceSettingsModel
{
    public int Id { get; set; }

    public int ShopId { get; set; }

    public int ShopSettingsId { get; set; }

    [Display(Name = "Service type")]
    public string? ServiceTypeName { get; set; }

    [Display(Name = "Service implementation type")]
    public string? ImplementationTypeName { get; set; }

    [Display(Name = "Service assembly path")]
    public string? AssemblyPath { get; set; }

    [Display(Name = "Service assembly path with implementation factory")]
    public string? ServiceProviderPath { get; set; }

    [Display(Name = "All data in json")]
    public string? JsonValue { get; set; }
}
