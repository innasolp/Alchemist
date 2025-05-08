using Alchemist.Product.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Category.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.Import.Products.Data;
using Alchemist.Import.Categories.Data;
using Message.Interfaces;
using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Import.Interfaces;

namespace Alchemist.Product.Import.Background;

public class ShopImportWorker : BackgroundService
{
    private readonly ILogger<ShopImportWorker> _logger;
    private readonly List<IImportService> _shopProductImportServices;
    private readonly List<IShopCategoryImportService> _shopCategoryImportServices;
    private readonly IMessageReceiver _messageReceiver;
    private readonly IShopDataService _shopDataService;
    private readonly IMessageSender _itemMessageSender;
    private readonly IProductDataHandler _productDataHandler;
    private readonly ICategoryDataHandler _categoryDataHandler;

    public ShopImportWorker(ILogger<ShopImportWorker> logger,
        [FromKeyedServices(ShopImportWorkerKeys.DataMessageReceiverKey)]
        IMessageReceiver messageReceiver,
        IShopDataService shopDataService,
        IEnumerable<IImportService> shopProductImportServices,
        [FromKeyedServices(ShopImportWorkerKeys.ShopsMessageSenderKey)]
        IMessageSender itemMessageSender,
        IProductDataHandler productDataHandler)
    {
        _logger = logger;
        _messageReceiver = messageReceiver;
        _shopDataService = shopDataService;
        _itemMessageSender = itemMessageSender;
        _productDataHandler = productDataHandler;

        _shopProductImportServices = [.. shopProductImportServices];
        //todo _shopProductImportServices.ForEach(s => s.ItemHandled += ServiceItemHandledAsync);

        _messageReceiver.On<Shop>(Messages.ReceiveShopCreated, OnShopCreated);       

        _messageReceiver.On<ShopCategory>(Messages.ReceiveCategoryAdded, OnShopCategoryAdded);
    }

    public ShopImportWorker(ILogger<ShopImportWorker> logger,
        [FromKeyedServices(ShopImportWorkerKeys.DataMessageReceiverKey)]
        IMessageReceiver messageReceiver,
        IShopDataService shopDataService,
        IEnumerable<IImportService> shopProductImportServices,
        [FromKeyedServices(ShopImportWorkerKeys.ShopsMessageSenderKey)]
        IMessageSender itemMessageSender,
        IProductDataHandler productDataHandler,
        ICategoryDataHandler categoryDataHandler,
        IEnumerable<IShopCategoryImportService> shopCategoryImportServices)
        : this(logger, messageReceiver, shopDataService, shopProductImportServices, itemMessageSender, productDataHandler)
    {
        _categoryDataHandler = categoryDataHandler;

        _shopCategoryImportServices = [.. shopCategoryImportServices];
        _shopCategoryImportServices.ForEach(s => s.NewCategoryLoad += NewCategoryLoadAsync);
    }

    private async Task NewCategoryLoadAsync(object? sender, NewCategoryEventArgs e)
    {
        if (sender is not IShopCategoryImportService service || e.NewCategory == null)
            return;

        //todo
        //var result = await _categoryDataHandler.HandleItem(e.NewCategory, service.ShopModel.Id);

        //var categoryModel = new ImportCategory { Category = e.NewCategory.Name, ItemId = e.NewCategory.Id, ShopId = service.ShopModel.Id, Status = result };

        //await _itemMessageSender.Send(categoryModel, Messages.SendCategoryItem);
    }

    private async Task ServiceItemHandledAsync(object? sender, ItemHandledEventArgs e)
    {
        if (sender is not IImportService service)
            return;

        //todo
        //var result = await _productDataHandler.HandleItem(e.Item, service.ShopModel.Id);

        //var productItemModel = new ImportProduct { Name = e.Item.Name, ShopName = service.Name, Url = e.Item.ItemUrl, Status = result };
        //await _itemMessageSender.Send(productItemModel, Messages.SendProductItem);
    }

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

        var allServices = new List<IImportService>(_shopProductImportServices.OfType<IImportService>().Union(_shopCategoryImportServices ?? []));

        _logger.LogInformation("Import services initialized.");

        try
        {
            //todo
            //await Task.WhenAll(_shopCategoryImportServices.Select(s => s.ShopModel.InitShopModelAsync(_shopDataService)));
            //await Task.WhenAll(_shopProductImportServices.Select(s => s.ShopModel.InitShopModelAsync(_shopDataService)));
            //await Task.WhenAll(_shopProductImportServices.Select(s => s.ProductShopModel.SetShopProductCategoriesAsync(_shopDataService)));

            await Parallel.ForEachAsync(allServices, (s, t) => new ValueTask(s.Start(stoppingToken)));
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
