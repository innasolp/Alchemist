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
using Microsoft.VisualStudio.Threading;
using Alchemist.Product.Interfaces;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Products.Interfaces;

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
    private readonly ICategoryItemHandler _categoryDataHandler;

    private readonly List<IImportService>  _services = [];


    public ShopImportWorker(ILogger<ShopImportWorker> logger,
        [FromKeyedServices(ShopImportWorkerKeys.DataMessageReceiverKey)]
        IMessageReceiver messageReceiver,
        IShopDataService shopDataService,
        IEnumerable<ISettingsAdapter> settingsAdapters,
        IEnumerable<IShopImportServiceFactory> shopImportFactories,
        [FromKeyedServices(ShopImportWorkerKeys.ShopsMessageSenderKey)]
        IMessageSender itemMessageSender,
        IProductItemHandler productDataHandler)
    {
        _logger = logger;
        _messageReceiver = messageReceiver;
        _shopDataService = shopDataService;
        _itemMessageSender = itemMessageSender;
        _productDataHandler = productDataHandler;
        _settingsAdapters = settingsAdapters;

        _shopServiceFactories = shopImportFactories;
        //todo _shopProductImportServices.ForEach(s => s.ItemHandled += ServiceItemHandledAsync);

        _messageReceiver.On<Shop>(Messages.ReceiveShopCreated, OnShopCreated);       

        _messageReceiver.On<ShopCategory>(Messages.ReceiveCategoryAdded, OnShopCategoryAdded);
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
        ICategoryItemHandler categoryDataHandler)
        : this(logger, messageReceiver, shopDataService, settingsAdapters, shopImportFactories, itemMessageSender, productDataHandler)
    {
        _categoryDataHandler = categoryDataHandler;        
    }

    //todo
    //private async Task NewCategoryLoadAsync(object? sender, NewCategoryEventArgs e)
    //{
    //    if (sender is not IShopCategoryImportService service || e.NewCategory == null)
    //        return;

    // //todo
    //    //var result = await _categoryDataHandler.HandleItem(e.NewCategory, service.ShopModel.Id);

    //    //var categoryModel = new ImportCategory { Category = e.NewCategory.Name, ItemId = e.NewCategory.Id, ShopId = service.ShopModel.Id, Status = result };

    //    //await _itemMessageSender.Send(categoryModel, Messages.SendCategoryItem);
    //}

    //todo
    //private async Task ServiceItemHandledAsync(object? sender, ItemHandledEventArgs e)
    //{
    //    if (sender is not IImportService service)
    //        return;

    //    //todo
    //    //var result = await _productDataHandler.HandleItem(e.Item, service.ShopModel.Id);

    //    //var productItemModel = new ImportProduct { Name = e.Item.Name, ShopName = service.Name, Url = e.Item.ItemUrl, Status = result };
    //    //await _itemMessageSender.Send(productItemModel, Messages.SendProductItem);
    //}

    private void OnShopCategoryAdded(ShopCategory shopCategory)
    {
        //todo
        //var service = _shopProductImportServices.OfType<IImportService>().FirstOrDefault(s => s.ShopModel.Id == shopCategory.ShopId);
        //service?.ProductShopModel.Categories.Add(shopCategory);
    }    

    private void OnShopCreated(Shop shop)
    {
        //todo
        //var service = _shopProductImportServices.FirstOrDefault(s => s.ShopModel.Name.Equals(shop.Name, StringComparison.CurrentCultureIgnoreCase));
        //if (service != null)
        //    service.ShopModel.Id = shop.Id;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _messageReceiver.Start();

        await _itemMessageSender.Start();

        _logger.LogInformation("Import service connected to rabbitMq.");

        var joinableTaskFactory = new JoinableTaskFactory(new JoinableTaskContext());

        var allShopImportSettings = new List<IShopImportSettings>();
        foreach(var adapter in _settingsAdapters)
        {
            var shopImportSettings = await adapter.GetAllShopImportSettings();
            var newSettings = shopImportSettings.Where(s => !allShopImportSettings.Any(s2 => s2.Name == s.Name));
            allShopImportSettings.AddRange(newSettings);
        }

        
        foreach (var shopImportSettings  in allShopImportSettings)
        {
            var serviceFactory = _shopServiceFactories.First(f => f.ServiceImplementationType.Name == shopImportSettings.ImportService.ImplementationTypeName);
            if (serviceFactory == null) continue;
            
            IShopModel shopModel = shopImportSettings.ShopSettingType == ShopSettingType.Product
                ? await _shopDataService.CreateProductShopModelAsync(shopImportSettings as IProductShopImportSettings)
                : await _shopDataService.CreateShopModelAsync(shopImportSettings);

            var shopImportService = serviceFactory.Create(shopModel, shopImportSettings);
            _services.Add(shopImportService);
        }

        _logger.LogInformation("Import services initialized.");

        try
        {
            await Parallel.ForEachAsync(_services, (s, t) => new ValueTask(s.Start(stoppingToken)));
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
        //todo _shopProductImportServices.ForEach(s => s.ItemHandled -= ServiceItemHandledAsync);
        base.Dispose();
    }
}
