using Alchemist.Product.ImportItem.Handler;
using Alchemist.Product.ImportItem.Interfaces;
using Message.Interfaces;

namespace Alchemist.Product.Import.DBService;

public class ImportItemHandlerService : BackgroundService
{
    private readonly ILogger<ImportItemHandlerService> _logger;

    private readonly IMessageReceiver _messageReceiver;

    private readonly  IItemHandler<IImportProductItem> _productItemHandler;

    private readonly  IItemHandler<IImportCategoryItem> _categoryItemHandler;

    public ImportItemHandlerService(ILogger<ImportItemHandlerService> logger,
        [FromKeyedServices(ItemHaldlerKeys.ProductRoutingKey)] string productRoutingKey,
        [FromKeyedServices(ItemHaldlerKeys.CategoryRoutingKey)] string categoryRoutingKey,
        IMessageReceiver messageReceiver,
        IItemHandler<IImportProductItem> productItemHandler,
        IItemHandler<IImportCategoryItem> categoryItemHandler
    )
    {
        _logger = logger;
        _messageReceiver = messageReceiver;
        _productItemHandler = productItemHandler;
        _categoryItemHandler = categoryItemHandler;

        _messageReceiver.On<IImportProductItem>(productRoutingKey, OnHandleProductItem);
        _messageReceiver.On<IImportCategoryItem>(categoryRoutingKey, OnHandleCategoryItem);
    }

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
