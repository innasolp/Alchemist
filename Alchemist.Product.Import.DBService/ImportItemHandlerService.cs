using Alchemist.Product.ImportItem.Handler;
using Alchemist.Product.ImportItem.Interfaces;
using Message.Interfaces;

namespace Alchemist.Product.Import.DBService;

public class ImportItemHandlerService(ILogger<ImportItemHandlerService> logger,
    [FromKeyedServices(ItemHaldlerKeys.ProductRoutingKey)] string productRoutingKey,
    [FromKeyedServices(ItemHaldlerKeys.CategoryRoutingKey)] string categoryRoutingKey,
    IMessageReceiver messageReceiver,
    IItemHandler<IImportProductItem> productItemHandler,
    IItemHandler<IImportCategoryItem> categoryItemHandler
    ) : BackgroundService
{
    private readonly ILogger<ImportItemHandlerService> _logger = logger;

    private readonly IMessageReceiver _messageReceiver = messageReceiver;

    private readonly  IItemHandler<IImportProductItem> _productItemHandler = productItemHandler;

    private readonly  IItemHandler<IImportCategoryItem> _categoryItemHandler = categoryItemHandler;

    private readonly string _productRoutingKey = productRoutingKey;

    private readonly string _categoryRoutingKey = categoryRoutingKey;

    private async Task OnHandleCategoryItem(IImportCategoryItem item)
    {
        await _categoryItemHandler.HandleItem(item);
    }

    private async Task OnHandleProductItem(IImportProductItem item)
    {
        await _productItemHandler.HandleItem(item);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _messageReceiver.Start();

            _logger.LogInformation("Import service connected to messaging host.");

            _messageReceiver.On<IImportProductItem>(_productRoutingKey, OnHandleProductItem);
            _messageReceiver.On<IImportCategoryItem>(_categoryRoutingKey, OnHandleCategoryItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
        }
    }
}
