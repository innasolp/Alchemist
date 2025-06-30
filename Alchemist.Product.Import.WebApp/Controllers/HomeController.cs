using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Entities;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;
using Message.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using ModelHelper = Alchemist.Product.Import.WebApp.Models.ModelHelper;

namespace Alchemist.Product.Import.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly IImportFacade _importFacade;

    private readonly IMessageReceiver _shopEventReceiver;

    private readonly ISettingsDataAdapter _settingsDataAdapter;

    private readonly IShopDataService _shopDataService;

    public HomeController(ILogger<HomeController> logger, IShopDataService shopDataService, ISettingsDataAdapter settingsDataAdapter, IImportFacade importFacade, IMessageReceiver shopEventReceiver)
    {
        _logger = logger;
        _shopDataService = shopDataService;
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

    private IndexViewModel? GetDefaultIndexViewModel()
    {
        var shopGuid = GetCurrentShopGuid();
        var tab = GetCurrentTabOrDefault();

        return GetIndexViewModel(shopGuid, tab);
    }

    private IndexViewModel? GetIndexViewModel(Guid? shopGuid, TabType tab)
    {
        var shopImports = _importFacade.GetShops();

        if (shopImports.Count == 0)
            return new IndexViewModel
            {
                Shops = [],
                SelectedTab = tab
            };

        ShopImportModel? shopImport;
        if (shopGuid == null)
            shopImport = shopImports.First();
        else if (!_importFacade.TryGetShopImport((Guid)shopGuid, out shopImport))
            return null;

        var shops = shopImports.Select(s => s.Shop).ToList();

        return new IndexViewModel
        {
            SelectedShopImport = shopImport,
            SelectedTab = tab,
            SelectedTabModel = shopImport.GetTab(tab),
            Shops = [.. shops.OfType<ShopModel>()]
        };
    }

    [ProducesResponseType<ViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Index()
    {
        var shopGuid = GetCurrentShopGuid();
        var tab = GetCurrentTabOrDefault();

        var viewModel = shopGuid != null
            ? GetIndexViewModel((Guid)shopGuid, tab)
            : GetDefaultIndexViewModel();

        if (viewModel.SelectedShopImport != null)        
            SetCurrentShopGuid(viewModel.SelectedShopImport.ShopGuid);          
        else        
            viewModel.SelectedShopImport = _importFacade.CreateDefaultShopImport();        

        viewModel.SelectedTabModel = viewModel.SelectedShopImport.GetTab(viewModel.SelectedTab);

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

            var viewModel = GetIndexViewModel(shopGuid, (TabType)tab);

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

        var settings = shopImport.GetLastSelectedSettings((TabType)tab);
        if (settings == null) return Ok(false);

        IShopServicesSettingsModel modelFromJson;
        try
        {
            modelFromJson = ModelHelper.DeserializeWithNumberHandling(json, settings.GetType()) as IShopServicesSettingsModel;
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
            //todo
            return Ok(true);

        //todo
        return Ok(!modelFromJson.FieldsEquals(originalSettings, false));
    }

    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateShops()
    {
        try
        {
            var shops = await _shopDataService.GetShops();
            var shopModels = await _importFacade.LoadShops(shops);
            return await Task.FromResult(new OkObjectResult(shopModels.Select(si => si.Shop).ToList()));
        }
        catch (Exception e)
        {
            return new ObjectResult(e) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    public IActionResult ShopList([ModelBinder(typeof(ModelJsonEnumerableBinder))]  IEnumerable<IShopModel> shops)
    {
        return PartialView("~/Views/Home/_ShopListPartial.cshtml", shops);
    }

    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    public IActionResult TabsMenu(Guid shopGuid, int tab)
    {
        var shopImports = _importFacade.GetShops();

        ShopImportModel shopImport;
        
        if (shopGuid == Guid.Empty)
            shopImport = shopImports.First();
        else if (shopImports.Count == 0 || !_importFacade.TryGetShopImport(shopGuid, out shopImport))        
            shopImport = _importFacade.CreateDefaultShopImport();        

        return PartialView("~/Views/Home/_TabsMenuPartial.cshtml", shopImport.GetTab((TabType)tab));
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
            else if (shopImports.Count == 0 || !_importFacade.TryGetShopImport((Guid)shopGuid, out shopImport))            
                shopImport = _importFacade.CreateDefaultShopImport();

            var tabModel = shopImport.GetTab((TabType)tab);

            if((TabType)tab == TabType.Shop)
            {
                IShopServicesSettingsModel selectedSettings = shopImport.ShopSettingTabs.SelectedSettingsTab == Alchemist.Import.Settings.Interfaces.ShopSettingType.Product
                    ? shopImport.ShopSettingTabs.ShopProductsSettings
                    : shopImport.ShopSettingTabs.ShopCategoriesSettings;

                if(selectedSettings.IsEmpty())
                {
                    var shopImportSettings = await _settingsDataAdapter.GetShopImportSettings(shopImport.Shop.Id, shopImport.ShopSettingTabs.SelectedSettingsTab);
                    
                    if(shopImportSettings != null)
                        selectedSettings.Update(shopImportSettings);
                }
            }

            return PartialView($"~/Views/Home/{tabView}.cshtml", tabModel);
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
