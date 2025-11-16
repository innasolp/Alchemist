using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Product.ImportSettingsWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.ImportSettingsWebApp.Infrastructure;

internal class ShopImportSettingsFacade(ISettingsDataAdapter productSettingsDataAdapter,
    ISettingsDataAdapter categorySettingsDataAdapter, Controller controller) 
    : SettingsFacade(productSettingsDataAdapter, categorySettingsDataAdapter, controller)
{
    private async Task<T> SaveShopImportSettingsAsync<T>(T data, Action<T, T> updateFields)
        where T : ShopImportSettingsModel
    {
        var existing = await GetShopImportSettingsAsync(data.ShopId, data.ShopSettingType);

        var sessionShopSettings = await Controller.HttpContext.Session.GetShopImportSettingsFromSessionAsync() as T;

        T toSave;
        if (existing is T existingShopSettings)
        {
            existingShopSettings.UpdateShopImportSettingsCore(data);

            updateFields(existingShopSettings, data);

            if (sessionShopSettings != null && sessionShopSettings.ShopId == data.ShopId)
                existingShopSettings.UpdateServices(sessionShopSettings.Services);

            toSave = existingShopSettings;
        }
        else
        {
            toSave = sessionShopSettings != null && sessionShopSettings.ShopId == data.ShopId
                ? sessionShopSettings
                : data;

            toSave.UpdateShopImportSettingsCore(data);
            updateFields(toSave, data);

            if (sessionShopSettings != null && sessionShopSettings.ShopId == data.ShopId)
                toSave.UpdateServices(sessionShopSettings.Services);
        }

        await SaveAsync(toSave);

        Controller.HttpContext.Session.SetImportSettingToSession(toSave);

        return toSave;
    }

    public async Task<ProductShopImportSettingsModel> SaveProductShopSettings(ProductShopImportSettingsModel data)
    {
        return await SaveShopImportSettingsAsync(data,
           (target, source) => target.UpdateProductShopImportSettingsCore(source));
    }

    public async Task<CategoryShopImportSettingsModel> SaveCategoryShopSettings(CategoryShopImportSettingsModel data)
    {
        return await SaveShopImportSettingsAsync(data,
            (target, source) => target.UpdateCategoryShopImportSettingsCore(source));
    }

    private async Task<bool> IsSettingsChangedAsync<T>(T data, Func<T, bool> isEmpty, Func<T, T?, bool> inputFieldsEquals,
        Func<T, T, bool>? additionalFieldsEquals = null)
        where T : ShopImportSettingsModel
    {
        try
        {
            var existingShopSettings = await GetShopImportSettingsAsync(data.ShopId, data.ShopSettingType)
                as T;

            if (await Controller.HttpContext.Session.GetShopImportSettingsFromSessionAsync() is T sessionShopSettings &&
                sessionShopSettings != null && sessionShopSettings.ShopId == data.ShopId)
            {
                return existingShopSettings == null
                    ? !(isEmpty(data) && sessionShopSettings.ShopImportSettingsIsEmpty())
                    : !(inputFieldsEquals(data, existingShopSettings)
                      && additionalFieldsEquals?.Invoke(sessionShopSettings, existingShopSettings) != false
                      && sessionShopSettings.Services.ServicesAreEquals(existingShopSettings.Services));
            }
            else
            {
                return existingShopSettings == null
                    ? !isEmpty(data)
                    : !inputFieldsEquals(data, existingShopSettings);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    internal async Task<bool> IsProductShopSettingsChangedAsync(ProductShopImportSettingsModel data)
    {
        return await IsSettingsChangedAsync(data, (settings) => settings.IsEmpty(),
            (target, source) => target.ProductShopSettingsFieldsEquals(source),
            (target, source) => target.RootCategoriesEquals(source));
    }

    internal async Task<bool> IsCategoryShopSettingsChangedAsync(CategoryShopImportSettingsModel data)
    {
        return await IsSettingsChangedAsync(data, (settings) => settings.IsEmpty(),
            (target, source) => target.CategoryShopSettingsFieldsEquals(source));
    }
}
