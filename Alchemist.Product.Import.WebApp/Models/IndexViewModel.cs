using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;

namespace Alchemist.Product.Import.WebApp.Models;

public class IndexViewModel
{
    public List<ShopModel> Shops { get; set; }

    public ShopImportModel SelectedShopImport { get; set; }

    public TabType SelectedTab { get; set; }

    public SettingsModelBase SelectedTabModel { get; set; }
}
