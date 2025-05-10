using Alchemist.Product.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Message.Interfaces;
using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Factory;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Model;
using Alchemist.Import.Settings.Extensions;

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

    protected List<IShopModel> ShopModels { get; } = [];

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

        _messageReceiver.On<Shop>(Messages.ReceiveShopCreated, OnShopCreatedAsync);

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

    private async Task CategoryHandledAsync(object sender, ICategory item, IShopModel shopModel, ItemProcessStatus processStatus)
    {
        var categoryModel = new ImportCategory { Category = item.Name, ItemId = item.Id, Status = processStatus };

        if (shopModel is IShop shop) categoryModel.ShopId = shop.Id;

        await _itemMessageSender.Send(categoryModel, Messages.SendCategoryItem);
    }


    private async Task ProductItemHandledAsync(object sender, IProductItem item, IShopModel shopModel, ItemProcessStatus processStatus)
    {        
        var productItemModel = new ImportProduct { Name = item.Name, ShopName = shopModel.Name, Url = item.ItemUrl, Status = processStatus }; 

        await _itemMessageSender.Send(productItemModel, Messages.SendProductItem);
    }

    private void OnShopCategoryAdded(ShopCategory shopCategory)
    {
        var shop = ShopModels.OfType<ProductShopModel>().FirstOrDefault(s => s.Id == shopCategory.ShopId);
        shop?.Categories.Add(new ProductShopCategoryModel { Category = shopCategory.Category, ItemId = shopCategory.ItemId });
    }    

    private async Task OnShopCreatedAsync(Shop newShop)
    {        
        var shopModel = ShopModels.FirstOrDefault(s => s.Name.Equals(newShop.Name, StringComparison.CurrentCultureIgnoreCase));
        if (shopModel != null)
        {
            if (shopModel is IShop shop) shop.Id = newShop.Id;
            return;
        }

        var productShopSettings = await _settingsDataService.GetShopSettings(newShop.Id, ShopSettingType.Product);
        var categoryShopSettings = await _settingsDataService.GetShopSettings(newShop.Id, ShopSettingType.Category);

        if(productShopSettings != null)
            await CreateShopImportServiceAsync(productShopSettings);

        if(categoryShopSettings != null)
            await CreateShopImportServiceAsync(categoryShopSettings);
    }

    private async Task CreateShopImportServiceAsync(IShopSettings shopSettings)
    {
        IShopImportSettings shopImportSettings = shopSettings.Type == ShopSettingType.Product
             ? await shopSettings.GetShopImportSettings<ProductShopImportSettings, ImportServiceSettings>(_settingsDataService.GetChildSettings)
             : await shopSettings.GetShopImportSettings<CategoryShopImportSettings, ImportServiceSettings>(_settingsDataService.GetChildSettings); 
        
        var serviceFactory = _shopServiceFactories.First(f => f.ServiceImplementationType.Name == shopImportSettings.ImportService.ImplementationTypeName);

        IShopModel shopModel = shopImportSettings.ShopSettingType == ShopSettingType.Product
            ? await _shopDataService.CreateProductShopModelAsync(shopImportSettings as IProductShopImportSettings)
            : await _shopDataService.CreateShopModelAsync(shopImportSettings);

        var service = await CreateServiceForShopImportSettingsAsync(shopImportSettings, shopModel);

        Services.Add(service); 
        
        ShopModels.Add(shopModel);        
    }

    private async Task<IImportService> CreateServiceForShopImportSettingsAsync(IShopImportSettings shopImportSettings, IShopModel shopModel)
    {
        var serviceFactory = _shopServiceFactories.First(f => f.ServiceImplementationType.Name == shopImportSettings.ImportService.ImplementationTypeName);
        if (serviceFactory == null) 
            return await Task.FromResult(default(IImportService));        

        return await Task.FromResult(serviceFactory.Create(shopModel, shopImportSettings));
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
                var serviceFactory = _shopServiceFactories.First(f => f.ServiceImplementationType.Name == shopImportSettings.ImportService.ImplementationTypeName);
                if (serviceFactory == null) continue;

                IShopModel shopModel = shopImportSettings.ShopSettingType == ShopSettingType.Product
                    ? await _shopDataService.CreateProductShopModelAsync(shopImportSettings as IProductShopImportSettings)
                    : await _shopDataService.CreateShopModelAsync(shopImportSettings);

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
