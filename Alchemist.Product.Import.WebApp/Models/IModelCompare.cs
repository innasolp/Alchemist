using Alchemist.Product.Import.Model;

namespace Alchemist.Product.Import.WebApp.Models;

public interface IModelCompare
{
    bool FieldsEquals(object model);

    bool AllEquals(object model);

    bool InnersEquals(object model);
}

public partial class ShopSettingsModel : IModelCompare
{
    public virtual bool AllEquals(object model)
    {
        return FieldsEquals(model) && InnersEquals(model);
    }

    public abstract bool FieldsEquals(object model);

    public virtual bool InnersEquals(object model)
    {
        if (model is not ShopSettingsModel shopSettings)
            return false;

        return ((ImportService == null && shopSettings.ImportService == null) || ImportService?.AllEquals(shopSettings.ImportService) == true)
                      && ((BrowserDataLoader == null && shopSettings.BrowserDataLoader == null) || BrowserDataLoader?.AllEquals(shopSettings.BrowserDataLoader) == true)
                      && ((RequestHeaders == null && shopSettings.RequestHeaders == null) || RequestHeaders?.AllEquals(shopSettings.RequestHeaders) == true)
                      && ((WebLoader == null && shopSettings.WebLoader == null) || WebLoader?.AllEquals(shopSettings.WebLoader) == true)
                      && Services.Where(s => !ModelHelper.IsServiceSettingsPrimary(s.Name)).All(s => shopSettings.Services.Any(s1 => s1.AllEquals(s)));
    }
}

public partial class ProductShopSettingsModel : IModelCompare
{
    public override bool AllEquals(object model)
    {
        if (model is not ProductShopSettingsModel)
            return false;

        return FieldsEquals(model) && InnersEquals(model);
    }

    public override bool FieldsEquals(object model)
    {
        if (model is not ProductShopSettingsModel productShopSettings)
            return false;

        return ((string.IsNullOrEmpty(ProductUrlFormat) && string.IsNullOrEmpty(productShopSettings.ProductUrlFormat))
                || string.Equals(ProductUrlFormat, productShopSettings.ProductUrlFormat, StringComparison.InvariantCultureIgnoreCase))
            && ((string.IsNullOrEmpty(CategoryUrlFormat) && string.IsNullOrEmpty(productShopSettings.CategoryUrlFormat))
                || string.Equals(CategoryUrlFormat, productShopSettings.CategoryUrlFormat, StringComparison.InvariantCultureIgnoreCase));
    }

    public override bool InnersEquals(object model)
    {
        if (model is not ProductShopSettingsModel productShopSettings)
            return false;

        return
            RootCategories.Count == productShopSettings.RootCategories.Count
                      && RootCategories.OfType<ICategoryUrlModel>().All(c => productShopSettings.RootCategories.Any(r => r.Item == c.Item && r.Url == c.Url))
                      && base.InnersEquals(model);
    }
}

public partial class CategoryShopSettingsModel
{
    public override bool FieldsEquals(object model)
    {
        if (model is not CategoryShopSettingsModel categoryShopSettings)
            return false;

        return ((string.IsNullOrEmpty(CategorySourceUrl) && string.IsNullOrEmpty(categoryShopSettings.CategorySourceUrl))
                || string.Equals(CategorySourceUrl, categoryShopSettings.CategorySourceUrl, StringComparison.InvariantCultureIgnoreCase));
    }

    public override bool AllEquals(object model)
    {
        if (model is not CategoryShopSettingsModel)
            return false;

        return base.AllEquals(model);
    }

    public override bool InnersEquals(object model)
    {
        if (model is not CategoryShopSettingsModel)
            return false;

        return base.InnersEquals(model);
    }
}

public partial class ServiceSettingsModel : IModelCompare
{
    public bool AllEquals(object model)
    {
        return FieldsEquals(model);
    }

    public bool FieldsEquals(object model)
    {
        if (model is not ServiceSettingsModel serviceSettingsModel)
            return false;

        return Name == serviceSettingsModel.Name
           && ((string.IsNullOrEmpty(ServiceTypeName) && string.IsNullOrEmpty(serviceSettingsModel.ServiceTypeName))
             || string.Equals(ServiceTypeName, serviceSettingsModel.ServiceTypeName, StringComparison.CurrentCultureIgnoreCase))
            && ((string.IsNullOrEmpty(AssemblyPath) && string.IsNullOrEmpty(serviceSettingsModel.AssemblyPath))
             || string.Equals(AssemblyPath, serviceSettingsModel.AssemblyPath, StringComparison.CurrentCultureIgnoreCase))
            && ((string.IsNullOrEmpty(ServiceProviderPath) && string.IsNullOrEmpty(serviceSettingsModel.ServiceProviderPath))
             || string.Equals(ServiceProviderPath, serviceSettingsModel.ServiceProviderPath, StringComparison.CurrentCultureIgnoreCase))
             && ((string.IsNullOrEmpty(ImplementationTypeName) && string.IsNullOrEmpty(serviceSettingsModel.ImplementationTypeName))
             || string.Equals(ImplementationTypeName, serviceSettingsModel.ImplementationTypeName, StringComparison.CurrentCultureIgnoreCase));
    }

    public bool InnersEquals(object model)
    {
        return model is ServiceSettingsModel;
    }
}

