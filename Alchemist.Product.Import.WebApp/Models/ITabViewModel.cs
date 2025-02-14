namespace Alchemist.Product.Import.WebApp.Models;

public interface ITabViewModel
{
    bool IsActive { get; set; }

    string PartialViewName { get; }
}
