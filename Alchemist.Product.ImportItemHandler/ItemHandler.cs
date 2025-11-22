using Alchemist.Common;
using Alchemist.Exceptions;
using Message.Interfaces;

namespace Alchemist.Product.Import.Background.ImportItems;

internal abstract class ItemHandler<T, TItem, TMessageItem>(IMessageSender messageSender, string methodName, ItemProcessor<T, TMessageItem> itemProcessor) 
    : IItemHandler<T, ResultStatus>, IAsyncDisposable
{
    private readonly IMessageSender _messageSender = messageSender;

    private readonly string _methodName = methodName;

    public event AsyncEventHandler<ItemProcessEventArgs<T, ResultStatus>> ItemProcessed;

    private readonly ItemProcessor<T, TMessageItem> _itemProcessor = itemProcessor;

    public async Task<ResultStatus> HandleItem(T item, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_messageSender.IsConnected)
                await _messageSender.Start(cancellationToken);

            var importEntity = ConvertToImportEntity(item);
        
            await _messageSender.Send(importEntity, _methodName, cancellationToken);
            await InvokeItemProcessedAsync(item, ResultStatus.Success, cancellationToken);
            return ResultStatus.Success;
        }
        catch (Exception e)
        {
            await InvokeItemProcessedAsync(item,  ResultStatus.Warning);
            throw new WarningException($"Item {GetUrl(item)} proccessed with error.", e);
        }
    }

    protected abstract TItem ConvertToImportEntity(T item);

    protected abstract string GetUrl(T item);

    private async Task InvokeItemProcessedAsync(T item, ResultStatus itemProcessStatus, CancellationToken cancellationToken = default)
    {
        await _itemProcessor.ProcessItemAsync(item, itemProcessStatus, cancellationToken);
        
        var task = ItemProcessed?.Invoke(this, new ItemProcessEventArgs<T, ResultStatus>(item, itemProcessStatus, cancellationToken));
        if (task != null) await task;
    }

    public async ValueTask DisposeAsync()
    {
        if (_messageSender != null && _messageSender.IsConnected)
            await _messageSender.Stop();
    }
}
