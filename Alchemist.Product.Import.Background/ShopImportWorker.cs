using Alchemist.Product.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Message.Interfaces;
using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Product.Interfaces;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Import.Background.Settings;
using Alchemist.Product.Import.Background.Models;
using Alchemist.Import.Service.Factory.Interfaces;


namespace Alchemist.Product.Import.Background;

public class ShopImportWorker : BackgroundService
{
    private readonly ILogger<ShopImportWorker> _logger;
    private readonly IEnumerable<IShopImportServiceFactory> _shopServiceFactories;
    private readonly IMessageReceiver _messageReceiver;
    private readonly IShopDataService _shopDataService;
    private readonly IEnumerable<ISettingsAdapter> _settingsAdapters;
    private readonly IMessageSender _itemMessageSender;
    private readonly IProductItemHandler _productDataHandler;
    private readonly ICategoryItemHandler? _categoryDataHandler;
    private readonly IShopSettingsDataService _settingsDataService;

    protected List<IImportService> Services { get; } = [];

    private List<IShopItem> ShopModels { get; } = [];

    public ShopImportWorker(ILogger<ShopImportWorker> logger,
        [FromKeyedServices(ShopImportWorkerKeys.DataMessageReceiverKey)]
        IMessageReceiver messageReceiver,
        IShopDataService shopDataService,
        IEnumerable<ISettingsAdapter> settingsAdapters,
        IEnumerable<IShopImportServiceFactory> shopImportFactories,
        [FromKeyedServices(ShopImportWorkerKeys.ShopsMessageSenderKey)]
        IMessageSender itemMessageSender,
        IProductItemHandler productDataHandler,
        IShopSettingsDataService settingsDataService)
    {
        _logger = logger;
        _messageReceiver = messageReceiver;
        _shopDataService = shopDataService;
        _itemMessageSender = itemMessageSender;
        _productDataHandler = productDataHandler;
        _settingsAdapters = settingsAdapters;

        _shopServiceFactories = shopImportFactories;

        _productDataHandler.ItemProcessed += ProductItemHandledAsync;

        _messageReceiver.On<ShopSettings>(Messages.ReceiveShopSettingsCreated, OnShopSettingsCreatedAsync);       

        _messageReceiver.On<ShopCategory>(Messages.ReceiveCategoryAdded, OnShopCategoryAdded);
        _settingsDataService = settingsDataService;
    }

    public ShopImportWorker(ILogger<ShopImportWorker> logger,
        [FromKeyedServices(ShopImportWorkerKeys.DataMessageReceiverKey)]
        IMessageReceiver messageReceiver,
        IShopDataService shopDataService,
        IEnumerable<ISettingsAdapter> settingsAdapters,
        IEnumerable<IShopImportServiceFactory> shopImportFactories,
        [FromKeyedServices(ShopImportWorkerKeys.ShopsMessageSenderKey)]
        IMessageSender itemMessageSender,
        IProductItemHandler productDataHandler,
        ICategoryItemHandler categoryDataHandler,
        IShopSettingsDataService settingsDataService)
        : this(logger, messageReceiver, shopDataService, settingsAdapters, shopImportFactories, itemMessageSender, productDataHandler, settingsDataService)
    {
        _categoryDataHandler = categoryDataHandler;
        _categoryDataHandler.ItemProcessed += CategoryHandledAsync;
    }

    private async Task CategoryHandledAsync(object sender, IImportCategory item, ResultStatus processStatus)
    {
        try
        {
            if (!_itemMessageSender.IsConnected)
                await _itemMessageSender.Start();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return;
        }

        var categoryModel = new ImportCategory { Category = item.Category.Name, ItemId = item.Category.Id, Status = processStatus };

        if (item.CategoryShopModel is IShop shop) categoryModel.ShopId = shop.Id;

        await _itemMessageSender.Send(categoryModel, Messages.SendCategoryItem);
    }


    private async Task ProductItemHandledAsync(object sender, IImportProduct item, ResultStatus status)
    {
        try
        {
            if (!_itemMessageSender.IsConnected)
                await _itemMessageSender.Start();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return;
        }

        var productItemModel = new ImportProduct { Name = item.ProductItem.Name, ShopName = item.Shop.ShopName, Url = item.ProductItem.Url, Status = status }; 

        await _itemMessageSender.Send(productItemModel, Messages.SendProductItem);
    }

    private void OnShopCategoryAdded(ShopCategory shopCategory)
    {
        var shop = ShopModels.OfType<ProductShopModel>().FirstOrDefault(s => s.Id == shopCategory.ShopId);
        shop?.Categories.Add(new ProductShopCategoryModel { Category = shopCategory.Category, ItemId = shopCategory.ItemId });
    }    
    

    private async Task OnShopSettingsCreatedAsync(ShopSettings newShopSettings)
    {
        if (ShopModels.OfType<IShopModel>().Any(s => s.Id == newShopSettings.ShopId))
            return;

        _logger.LogInformation($"Handling of settings {newShopSettings.Name} for shop id={newShopSettings.ShopId} started.");

        var serviceSettings = await _settingsDataService.GetChildSettings(newShopSettings.Id);
        var shopImportSettings = newShopSettings.GetShopImportSettings(serviceSettings);
        
        var shopModel = await _shopDataService.GetShopModelAsync(shopImportSettings);

        var importServiceSettings = shopImportSettings.GetImportService();
        if (importServiceSettings == null) return;

        var serviceFactory = _shopServiceFactories.First(f => f.ServiceImplementationType.Name == importServiceSettings.ImplementationTypeName);
        var service = serviceFactory.Create(shopModel, shopImportSettings);
        
        ShopModels.Add(shopModel);

        Services.Add(service);

        _logger.LogInformation($"New service for shop {shopModel.ShopName} added");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _messageReceiver.Start();

            await _itemMessageSender.Start();
            
            _logger.LogInformation("Import service connected to messaging host.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }        

        try
        {
            var allShopImportSettings = new List<IShopImportSettings>();
            foreach (var adapter in _settingsAdapters)
            {
                var shopImportSettings = await adapter.GetAllShopImportSettings();
                var newSettings = shopImportSettings.Where(s => !allShopImportSettings.Any(s2 => s2.Name == s.Name));
                allShopImportSettings.AddRange(newSettings);
            }

            foreach (var shopImportSettings in allShopImportSettings)
            {
                var importServiceSettings = shopImportSettings.GetImportService();
                if (importServiceSettings == null)
                    continue;
                
                var serviceFactory = _shopServiceFactories.FirstOrDefault(f => f.ServiceImplementationType.Name == importServiceSettings.ImplementationTypeName);
                if (serviceFactory == null) continue;

                var shopModel = await _shopDataService.GetShopModelAsync(shopImportSettings);
                ShopModels.Add(shopModel);

                var shopImportService = serviceFactory.Create(shopModel, shopImportSettings);
                Services.Add(shopImportService);
            }
            
            _logger.LogInformation("Import services initialized.");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }        

        try
        {
            await Parallel.ForEachAsync(Services, (s, t) => new ValueTask(s.Start(stoppingToken)));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }    

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _messageReceiver.Stop();
        await _itemMessageSender.Stop();
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _productDataHandler.ItemProcessed -= ProductItemHandledAsync;
        
        if (_categoryDataHandler != null)
            _categoryDataHandler.ItemProcessed -= CategoryHandledAsync;
        
        base.Dispose();
    }
}
