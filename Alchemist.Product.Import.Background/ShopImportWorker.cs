using Alchemist.Product.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Category.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.Import.Products.Data;
using Alchemist.Import.Categories.Data;
using Message.Interfaces;
using Alchemist.Import.Products.Service;
using Alchemist.Import.Shop.Interfaces;
using Alchemist.Product.DataService.Interfaces;
using Alchemist.Common;

namespace Alchemist.Product.Import.Background;

public class ShopImportWorker : BackgroundService
{
    private readonly ILogger<ShopImportWorker> _logger;
    private readonly List<IShopProductImportService> _shopProductImportServices;
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
        IEnumerable<IShopProductImportService> shopProductImportServices,
        [FromKeyedServices(ShopImportWorkerKeys.ShopsMessageSenderKey)]
        IMessageSender itemMessageSender,
        IProductDataHandler productDataHandler)
    {
        _logger = logger;
        _messageReceiver = messageReceiver;
        _shopDataService = shopDataService;
        _itemMessageSender = itemMessageSender;
        _productDataHandler = productDataHandler;

        _shopProductImportServices = new List<IShopProductImportService>(shopProductImportServices);
        _shopProductImportServices.ForEach(s => s.ItemHandled += ServiceItemHandledAsync);

        _messageReceiver.On<Shop>(Messages.ReceiveShopCreated, OnShopCreated);

        _messageReceiver.On<ShopUrl>(Messages.ReceiveShopUrlSet, OnShopUrlSet);

        _messageReceiver.On<ShopCategory>(Messages.ReceiveCategoryAdded, OnShopCategoryAdded);
    }

    public ShopImportWorker(ILogger<ShopImportWorker> logger,
        [FromKeyedServices(ShopImportWorkerKeys.DataMessageReceiverKey)]
        IMessageReceiver messageReceiver,
        IShopDataService shopDataService,
        IEnumerable<IShopProductImportService> shopProductImportServices,
        [FromKeyedServices(ShopImportWorkerKeys.ShopsMessageSenderKey)]
        IMessageSender itemMessageSender,
        IProductDataHandler productDataHandler,
        ICategoryDataHandler categoryDataHandler,
        IEnumerable<IShopCategoryImportService> shopCategoryImportServices)
        : this(logger, messageReceiver, shopDataService, shopProductImportServices, itemMessageSender, productDataHandler)
    {
        _categoryDataHandler = categoryDataHandler;

        _shopCategoryImportServices = new List<IShopCategoryImportService>(shopCategoryImportServices);
        _shopCategoryImportServices.ForEach(s => s.NewCategoryLoad += NewCategoryLoadAsync);
    }

    private async Task NewCategoryLoadAsync(object? sender, NewCategoryEventArgs e)
    {
        if (sender is not IShopCategoryImportService service || e.NewCategory == null)
            return;

        var result = await _categoryDataHandler.HandleItem(e.NewCategory, service.ShopModel.ShopId);

        var categoryModel = new ImportCategory { Category = e.NewCategory.Name, ItemId = e.NewCategory.Id, ShopId = service.ShopModel.ShopId, Status = result };

        await _itemMessageSender.Send(categoryModel, Messages.SendCategoryItem);
    }

    private async Task ServiceItemHandledAsync(object? sender, ItemHandledEventArgs e)
    {
        if (sender is not IShopImportService service)
            return;

        var result = await _productDataHandler.HandleItem(e.Item, service.ShopModel.ShopId);

        var productItemModel = new ImportProduct { Name = e.Item.Name, ShopName = service.Name, Url = e.Item.ItemUrl, Status = result };
        await _itemMessageSender.Send(productItemModel, Messages.SendProductItem);
    }

    private void OnShopCategoryAdded(ShopCategory shopCategory)
    {
        var service = _shopProductImportServices.OfType<IShopProductImportService>().FirstOrDefault(s => s.ShopModel.ShopId == shopCategory.ShopId);
        service?.ProductShopModel.Categories.Add(shopCategory);
    }

    private void OnShopUrlSet(ShopUrl shopUrl)
    {
        var service = _shopProductImportServices.FirstOrDefault(s => s.ShopModel.ShopId == shopUrl.ShopId);
        if (service != null)
        {
            service.ProductShopModel.ProductUrl = shopUrl.ProductUrl;
            service.ProductShopModel.CategoryUrl = shopUrl.CategoryUrl;
        }
    }

    private void OnShopCreated(Shop shop)
    {
        var service = _shopProductImportServices.FirstOrDefault(s => s.ShopModel.ShopName.ToUpper() == shop.Name.ToUpper());
        if (service != null)
            service.ShopModel.ShopId = shop.Id;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _messageReceiver.Start();

        await _itemMessageSender.Start();

        _logger.LogInformation("Import service connected to rabbitMq.");

        var allServices = new List<IShopImportService>(_shopProductImportServices.OfType<IShopImportService>().Union(_shopCategoryImportServices ?? []));

        _logger.LogInformation("Import services initialized.");

        try
        {
            await Task.WhenAll(_shopCategoryImportServices.Select(s => s.ShopModel.InitShopModelAsync(_shopDataService)));
            await Task.WhenAll(_shopProductImportServices.Select(s => s.ProductShopModel.InitProductShopModelAsync(_shopDataService)));

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
        _shopProductImportServices.ForEach(s => s.ItemHandled -= ServiceItemHandledAsync);
        base.Dispose();
    }
}
