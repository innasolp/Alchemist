using Alchemist.Common;
using Alchemist.Product.Entities;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;
using Alchemist.Product.Interfaces;
using Message.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Alchemist.Product.Import.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly IImportFacade _importFacade;

    private readonly IMessageReceiver _shopEventReceiver;

    public HomeController(ILogger<HomeController> logger,
        IImportFacade importFacade,
        IMessageReceiver shopEventReceiver)
    {
        _logger = logger;
        _importFacade = importFacade;
        _shopEventReceiver = shopEventReceiver;

        _shopEventReceiver.On<Shop>(Messages.ReceiveShopCreated, OnShopCreated);
    }

    private void OnShopCreated(Shop shop)
    {
        _importFacade.AddNewShop(shop);
    }

    private Guid? GetCurrentShopGuid()
    {
        return ViewData["ShopGuid"] is Guid shopGuid ? shopGuid : (Guid?)null;
    }

    private void SetCurrentShopGuid(Guid? guid)
    {
        if (guid != null) ViewData["ShopGuid"] = guid;
    }

    private Guid? GetCurrentShopGuidOrDefault(List<ShopImportModel> shopImports)
    {
        var shopGuid = GetCurrentShopGuid();
        if (shopGuid == null)
        {
            shopGuid = shopImports.FirstOrDefault()?.ShopGuid;
            SetCurrentShopGuid(shopGuid);
        }

        return shopGuid;
    }

    private TabType? GetCurrentTab()
    {
        return ViewData["Tab"] is TabType tab ? tab : (TabType?)null;
    }

    private void SetCurrentTab(TabType? tab)
    {
        if(tab != null) ViewData["Tab"] = tab;
    }

    private TabType GetCurrentTabOrDefault()
    {
        var tab = GetCurrentTab();
        if (tab == null)
        {
            tab = TabType.Shop;
            SetCurrentTab(TabType.Shop);
        }
        return (TabType)tab;
    }

    private async Task<IndexViewModel> GetIndexViewModelAsync(Guid shopGuid, TabType tabType)
    {
        var shopImports = await _importFacade.LoadShops();

        var shops = shopImports.Select(s => s.Shop).ToList();
        return new IndexViewModel
        {
            SelectedShopImport = _importFacade.GetShopImport(shopGuid),
            SelectedTab = tabType,
            Shops = shops
        };
    }

    private async Task<IndexViewModel> GetDefaultIndexViewModelAsync()
    {
        var shopImports = await _importFacade.LoadShops();
        var shopGuid = GetCurrentShopGuidOrDefault(shopImports);

        var shops = shopImports.Select(s => s.Shop).ToList();
        return new IndexViewModel
        {
            SelectedShopImport = _importFacade.GetShopImport((Guid)shopGuid),
            SelectedTab = GetCurrentTabOrDefault(),
            Shops = shops
        };
    }

    public async Task<IActionResult> Index()
    {
        var shopGuid = GetCurrentShopGuid();
        var tab = GetCurrentTabOrDefault();

        var viewModel = shopGuid != null
            ? await GetIndexViewModelAsync((Guid)shopGuid, (TabType)tab)
            : await GetDefaultIndexViewModelAsync();

        return View("~/Views/Home/Index.cshtml", viewModel);
    }

    [Route("Home/Index/shopGuid={shopGuid}&tab={tab}")]
    public async Task<IActionResult> Index(Guid shopGuid, int tab)
    {
        SetCurrentShopGuid(shopGuid);
        SetCurrentTab((TabType)tab);

        var viewModel = await GetIndexViewModelAsync(shopGuid, (TabType)tab);

        return View(viewModel);
    }

    [HttpPost]
    public bool SaveTabSettings(Guid shopGuid, int tab, string json)
    {
        if (string.IsNullOrEmpty(json)) return false;

        if (!_importFacade.TryGetShopImport(shopGuid, out var shopImport))
            return false;

        var settings = shopImport.GetSettings((TabType)tab, true);
        settings.UpdateFromJson(json);

        return true;
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult ShopSettingTabs()
    {
        return PartialView();
    }


    [HttpPost]
    public bool SaveShopSettings(Guid shopGuid, int shopSettingType, string json)
    {
        if (string.IsNullOrEmpty(json)) return false;

        ShopSettingsModel? shopSettings = (ShopSettingType)shopSettingType == ShopSettingType.Product
            ?  json.DeserializeWithNumberHandling<ProductShopSettingsModel>()
            : json.DeserializeWithNumberHandling<CategoryShopSettingsModel>();
        if (shopSettings == null)
            throw new InvalidOperationException("Invalid json for shop settings");

        var shopSettingsModel =_importFacade.GetShopSettings(shopGuid, shopSettings.ShopSettingType)
            ?? _importFacade.CreateShopImportSettings(shopSettings.ShopSettingType, shopGuid);

        shopSettingsModel?.Update(shopSettings);

        return true;
    }

    [HttpPost]
    public bool SetShopSettings(Guid shopGuid, int shopSettingType)
    {
        if ((ShopSettingType)shopSettingType == ShopSettingType.Service)
            throw new InvalidOperationException("Invalid json for shop settings");

        if (!_importFacade.TryGetShopImport(shopGuid, out var shopImport))
            return false;

        shopImport.ShopSettingTabs.SelectedSettingsTab = (ShopSettingType)shopSettingType;

        return shopImport != null;
    }

    private IActionResult ServiceSettings(Guid shopGuid, Guid shopSettingsGuid, string serviceSettingsName)
    {
        var serviceSettingsModel = _importFacade.GetServiceSettingsModel(shopGuid, shopSettingsGuid, serviceSettingsName)
            ?? _importFacade.CreateServiceSettingsModel(shopGuid, shopSettingsGuid, serviceSettingsName);

        return serviceSettingsModel == null
            ? throw new InvalidDataException($"No data for shop {shopGuid} and settings {shopSettingsGuid}")
            : (IActionResult)PartialView("~/Views/Home/ServiceSettings.cshtml", serviceSettingsModel);
    }

    [HttpPost]
    public IActionResult ImportServiceSettings(Guid shopGuid, Guid shopSettingsGuid)
    {
        return ServiceSettings(shopGuid, shopSettingsGuid, nameof(ShopSettingsModel.ImportService));
    }


    [HttpPost]
    public IActionResult BrowserDataLoaderSettings(Guid shopGuid, Guid shopSettingsGuid)
    {
        return ServiceSettings(shopGuid, shopSettingsGuid, nameof(ShopSettingsModel.BrowserDataLoader));
    }

    [HttpPost]
    public IActionResult WebLoaderSettings(Guid shopGuid, Guid shopSettingsGuid)
    {
        return ServiceSettings(shopGuid, shopSettingsGuid, nameof(ShopSettingsModel.WebLoader));
    }

    [HttpPost]
    public bool SaveServiceSettings(ServiceSettingsModel data)
    {
        if (data == null || !_importFacade.TryGetShopImport(data.ShopGuid, out var shopImport))
            return false;

        var result = shopImport.ShopSettingTabs.GetShopSettingsByGuid(data.ShopSettingsGuid)?
                .UpdateServiceSettings(data);

        return result == true;
    }

    [HttpPost]
    public async Task<bool> SaveProductShopSettingsToDb(ProductShopSettingsModel productShopSettings)
    {
        if (productShopSettings == null) return false;

        if (!_importFacade.TryGetShopImport(productShopSettings.ShopGuid, out var shopImport) && shopImport != null)
            return false;

        shopImport.ShopSettingTabs.ShopProductsSettings.Update(productShopSettings);

        await _importFacade.Save(shopImport.ShopSettingTabs.ShopProductsSettings);

        return true;
    }

    [HttpPost]
    public async Task<bool> SaveCategoryShopSettingsToDb(CategoryShopSettingsModel categoryShopSettings)
    {
        if (categoryShopSettings == null) return false;

        if (!_importFacade.TryGetShopImport(categoryShopSettings.ShopGuid, out var shopImport) && shopImport != null)
            return false;

        shopImport.ShopSettingTabs.ShopCategoriesSettings.Update(categoryShopSettings);

        await _importFacade.Save(shopImport.ShopSettingTabs.ShopCategoriesSettings);

        return true;
    }

    public IActionResult ImportProducts()
    {
        return PartialView();
    }

    public IActionResult ImportCategories()
    {
        return PartialView();
    }
}
