using Alchemist.Common;
using Alchemist.Exceptions;
using Message.Interfaces;

namespace Alchemist.Product.Import.Background.ImportItems;

internal abstract class ItemHandler<T, TItem, TMessageItem>(IMessageSender messageSender, string methodName, ItemProcessor<T, TMessageItem> itemProcessor) : IItemHandler<T, ResultStatus> 
{
    private readonly IMessageSender _messageSender = messageSender;

    private readonly string _methodName = methodName;

    public event AsyncItemHandler<T, ResultStatus> ItemProcessed;

    private readonly ItemProcessor<T, TMessageItem> _itemProcessor = itemProcessor;

    public async Task<ResultStatus> HandleItem(T item)
    {
        try
        {
            if (!_messageSender.IsConnected)
                await _messageSender.Start();

            var importEntity = ConvertToImportEntity(item);
        
            await _messageSender.Send(importEntity, _methodName);
            await InvokeItemProcessedAsync(item, ResultStatus.Success);
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

    private async Task InvokeItemProcessedAsync(T item, ResultStatus itemProcessStatus)
    {
        await _itemProcessor.ProcessItemAsync(item, itemProcessStatus);
        
        await ItemProcessed?.Invoke(this, item, itemProcessStatus);
    }

}
