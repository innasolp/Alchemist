using System.ComponentModel;

namespace Alchemist.Product.Import.Model.Infrastructure;

public enum TabType
{
    [Description("Products")]
    [Category("ImportProducts")]
    Products = 1,

    [Description("Categories")]
    [Category("ImportCategories")]
    Categories = 2,

    [Description("Shop")]
    [Category("ShopSettingTabs")]
    Shop = 0
}
