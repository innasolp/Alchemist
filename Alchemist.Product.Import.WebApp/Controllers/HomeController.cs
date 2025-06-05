using Alchemist.Common;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Product.Entities;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;
using Message.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace Alchemist.Product.Import.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly IImportFacade _importFacade;

    private readonly IMessageReceiver _shopEventReceiver;

    private readonly ISettingsDataAdapter _settingsDataAdapter;

    public HomeController(ILogger<HomeController> logger, ISettingsDataAdapter settingsDataAdapter, IImportFacade importFacade, IMessageReceiver shopEventReceiver)
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

    private TabType? GetCurrentTab()
    {
        return ViewData["Tab"] is TabType tab ? tab : (TabType?)null;
    }

    private void SetCurrentTab(TabType? tab)
    {
        if (tab != null) ViewData["Tab"] = tab;
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

    private async Task<IndexViewModel?> GetDefaultIndexViewModelAsync()
    {
        var shopGuid = GetCurrentShopGuid();
        var tab = GetCurrentTabOrDefault();

        return await GetIndexViewModelAsync(shopGuid, tab);
    }

    private async Task<IndexViewModel?> GetIndexViewModelAsync(Guid? shopGuid, TabType tab)
    {
        var shopImports = _importFacade.GetShops();

        if (shopImports.Count == 0)
            return new IndexViewModel
            {
                Shops = [],
                SelectedTab = tab
            };

        ShopImportModel shopImport;

        if (shopGuid == null)
            shopImport = shopImports.First();
        else if (!_importFacade.TryGetShopImport((Guid)shopGuid, out shopImport))
            return await Task.FromResult(default(IndexViewModel));

        var shops = shopImports.Select(s => s.Shop).ToList();

        return new IndexViewModel
        {
            SelectedShopImport = shopImport,
            SelectedTab = tab,
            SelectedTabModel = shopImport.GetSettings(tab) ?? tab.CreateDefaultTabModel(),
            Shops = shops
        };
    }

    [ProducesResponseType<ViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Index()
    {
        var shopGuid = GetCurrentShopGuid();
        var tab = GetCurrentTabOrDefault();

        var viewModel = shopGuid != null
            ? await GetIndexViewModelAsync((Guid)shopGuid, tab)
            : await GetDefaultIndexViewModelAsync();

        if (viewModel.SelectedShopImport != null)
            SetCurrentShopGuid(viewModel.SelectedShopImport.ShopGuid);

        return View("~/Views/Home/Index.cshtml", viewModel);
    }

    [Route("Home/Index/shopGuid={shopGuid}&tab={tab}")]
    [ProducesResponseType<ViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType<BadRequestResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ActionName("Index")]
    public async Task<IActionResult> IndexRouteAsync(Guid shopGuid, int tab)
    {
        return await IndexAsync(shopGuid, tab);
    }

    [Route("Home/Index")]
    [ProducesResponseType<ViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType<BadRequestResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ActionName("Index")]
    public async Task<IActionResult> IndexFromQueryAsync([FromQuery] Guid shopGuid, [FromQuery] int tab)
    {
        return await IndexAsync(shopGuid, tab);
    }

    private async Task<IActionResult> IndexAsync(Guid shopGuid, int tab)
    {
        try
        {
            if (shopGuid == Guid.Empty || tab <0 || tab > (int)Enum.GetValues<TabType>().Max())
                return BadRequest();

            var viewModel = await GetIndexViewModelAsync(shopGuid, (TabType)tab);

            SetCurrentShopGuid(shopGuid);
            SetCurrentTab((TabType)tab);

            return viewModel != null ? View(viewModel) : NotFound(shopGuid);
        }
        catch (InvalidOperationException e)
        {
            _logger.LogError(e, $"Index({shopGuid},{tab})");
            return new ObjectResult(e) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

    [HttpPost]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> IsTabChanged(Guid shopGuid, int tab, string json)
    {
        if (string.IsNullOrEmpty(json)) return BadRequest(json);

        if (!_importFacade.TryGetShopImport(shopGuid, out var shopImport))
            return NotFound(shopGuid);

        var settings = shopImport.GetSettings((TabType)tab, true);
        if (settings == null) return Ok(false);

        SettingsModelBase modelFromJson;
        try
        {
            modelFromJson = json.DeserializeWithNumberHandling(settings.GetType());
        }
        catch(JsonException)
        {
            return BadRequest(json);
        }
        
        if (modelFromJson == null) return Ok(false);

        if ((TabType)tab != TabType.Shop)
            return Ok(!settings.Equals(modelFromJson));

        var originalSettings = await _settingsDataAdapter.GetShopImportSettings(shopImport.Shop.Id, (settings as ShopSettingsModel).ShopSettingType);
        if (originalSettings == null)
            return Ok(!modelFromJson.Equals(shopGuid.CreateShopSettings((settings as ShopSettingsModel).ShopSettingType)));

        return Ok(!modelFromJson.Equals(originalSettings));
    }

    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateShops()
    {
        try
        {
            var shops = await _importFacade.LoadShops();
            return await Task.FromResult(new OkObjectResult(shops.Select(si => si.Shop).ToList()));
        }
        catch (Exception e)
        {
            return new ObjectResult(e) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    public IActionResult ShopList(IEnumerable<ShopModel> shops)
    {
        return PartialView("~/Views/Home/_ShopListPartial.cshtml", shops);
    }

    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    public IActionResult TabsMenu(Guid shopGuid, int tab)
    {
        var shopImports = _importFacade.GetShops();

        if (shopImports.Count == 0)
            return PartialView("~/Views/Home/_TabsMenuPartial.cshtml", ((TabType)tab).CreateDefaultTabModel());

        ShopImportModel shopImport;

        if (shopGuid == Guid.Empty)
            shopImport = shopImports.First();
        else if (!_importFacade.TryGetShopImport(shopGuid, out shopImport))
            return PartialView("~/Views/Home/_TabsMenuPartial.cshtml", ((TabType)tab).CreateDefaultTabModel());

        return PartialView("~/Views/Home/_TabsMenuPartial.cshtml", shopImport.GetSettings((TabType)tab) ?? shopImport.CreateSettings((TabType)tab));
    }

    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoadTab(Guid? shopGuid, int tab, string tabView)
    {
        try
        {
            SetCurrentShopGuid(shopGuid);

            ShopImportModel shopImport;
            var shopImports = _importFacade.GetShops();

            if (shopGuid == null)
                shopImport = shopImports.First();
            else if (!_importFacade.TryGetShopImport((Guid)shopGuid, out shopImport))
                return PartialView($"~/Views/Home/{tabView}.cshtml", ((TabType)tab).CreateDefaultTabModel());


            var currentSettings = shopImport.GetSettings((TabType)tab);
            if (currentSettings == null)
            {
                currentSettings = shopImport.CreateSettings((TabType)tab);
                shopImport.SetSettings((TabType)tab, currentSettings);
            }

            if ((TabType)tab == TabType.Shop &&
                shopImport.GetSettings((TabType)tab, true) == null)
            {
                if (await _settingsDataAdapter.GetShopImportSettings(shopImport.Shop.Id, shopImport.ShopSettingTabs.SelectedSettingsTab) is ShopSettingsModel settings)
                    shopImport.SetSettings((TabType)tab, settings);
                else
                {
                    var currentShopSettings = shopImport.ShopGuid.CreateShopSettings(shopImport.ShopSettingTabs.SelectedSettingsTab);
                    shopImport.SetSettings((TabType)tab, currentShopSettings);
                }
            }

            return PartialView($"~/Views/Home/{tabView}.cshtml", currentSettings);
        }
        catch (Exception e)
        {
            return new ObjectResult(e) { StatusCode = StatusCodes.Status500InternalServerError };
        }
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
