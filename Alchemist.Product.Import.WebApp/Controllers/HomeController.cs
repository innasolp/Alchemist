using Alchemist.Common;
using Alchemist.Import.Settings.Adapter;
using Alchemist.Product.Entities;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;
using Message.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Alchemist.Product.Import.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly IImportFacade _importFacade;

    private readonly IMessageReceiver _shopEventReceiver;

    private readonly ISettingsDataAdapter<ProductShopSettingsModel, CategoryShopSettingsModel, ServiceSettingsModel> _settingsDataAdapter;

    public HomeController(ILogger<HomeController> logger,
    ISettingsDataAdapter<ProductShopSettingsModel, CategoryShopSettingsModel, ServiceSettingsModel> settingsDataAdapter,
        IImportFacade importFacade,
        IMessageReceiver shopEventReceiver)
    {
        _logger = logger;
        _importFacade = importFacade;
        _shopEventReceiver = shopEventReceiver;
        _settingsDataAdapter = settingsDataAdapter;

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

    private async Task<IndexViewModel> GetIndexViewModelAsync(Guid shopGuid, TabType tab)
    {
        var shopImports = await _importFacade.LoadShops();

        return await GetIndexViewModelAsync(shopImports, shopGuid, tab);
    }

    private async Task<IndexViewModel> GetDefaultIndexViewModelAsync()
    {
        var shopImports = await _importFacade.LoadShops();
        var shopGuid = GetCurrentShopGuidOrDefault(shopImports);
        var tab = GetCurrentTabOrDefault();

        return await GetIndexViewModelAsync(shopImports, (Guid)shopGuid, tab);        
    }

    private async Task<IndexViewModel> GetIndexViewModelAsync(IEnumerable<ShopImportModel> shopImports, Guid shopGuid, TabType tab)
    {
        var shops = shopImports.Select(s => s.Shop).ToList();
        if(!_importFacade.TryGetShopImport(shopGuid, out var shopImport))
            return await Task.FromResult(default(IndexViewModel));

        var currentSettings = shopImport.GetSettings(tab);
        if (currentSettings == null)
        {
            currentSettings = shopImport.CreateSettings(tab);
            shopImport.SetSettings(tab, currentSettings);
        }

        if (tab == TabType.Shop && 
            shopImport.GetSettings(tab,true) == null)
        {
            var settings = await _settingsDataAdapter.GetShopSettings(shopImport.Shop.Id, shopImport.ShopSettingTabs.SelectedSettingsTab) as ShopSettingsModel;
            if (settings != null)
                shopImport.SetSettings(tab, settings);
            else
            {
                var currentShopSettings = shopGuid.CreateShopSettings(shopImport.ShopSettingTabs.SelectedSettingsTab);
                shopImport.SetSettings(tab, currentShopSettings);
            }
        }

        return new IndexViewModel
        {
            SelectedShopImport = shopImport,
            SelectedTab = tab,
            SelectedTabModel = currentSettings,
            Shops = shops
        };
    }

    public async Task<IActionResult> Index()
    {
        var shopGuid = GetCurrentShopGuid();
        var tab = GetCurrentTabOrDefault();

        var viewModel = shopGuid != null
            ? await GetIndexViewModelAsync((Guid)shopGuid, tab)
            : await GetDefaultIndexViewModelAsync();

        return View("~/Views/Home/Index.cshtml", viewModel);
    }

    [Route("Home/Index/shopGuid={shopGuid}&tab={tab}")]
    public async Task<IActionResult> Index(Guid shopGuid, int tab)
    {
        SetCurrentShopGuid(shopGuid);
        SetCurrentTab((TabType)tab);

        try
        {
            var viewModel = await GetIndexViewModelAsync(shopGuid, (TabType)tab);

            return viewModel != null ? View(viewModel) : BadRequest();
        }
        catch(InvalidOperationException e)
        {
            _logger.LogError(e, $"Index({shopGuid},{tab})");
            return await Task.FromResult(BadRequest());
        }
    }

    [HttpPost]
    public bool SaveTabSettings(Guid shopGuid, int tab, string json)
    {
        if (string.IsNullOrEmpty(json)) return false;

        if (!_importFacade.TryGetShopImport(shopGuid, out var shopImport))
            return false;

        var settings = shopImport.GetSettings((TabType)tab, true);
        var modelFromJson = json.DeserializeWithNumberHandling(settings.GetType());
        settings.Update(modelFromJson);       

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
   
    public IActionResult ImportProducts()
    {
        return PartialView();
    }

    public IActionResult ImportCategories()
    {
        return PartialView();
    }
}
