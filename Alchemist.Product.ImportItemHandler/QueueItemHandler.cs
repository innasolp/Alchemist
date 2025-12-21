using Alchemist.BackgroundTaskQueue;
using Alchemist.Common;
using Import.Interfaces;
using Message.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Product.ImportItemHandler;

internal abstract class QueueItemHandler<T, TDataItem>(IBackgroundTaskQueue backgroundTaskQueue, 
    IMessageSender messageSender, 
    string processMethodName) 
    : IItemHandler<T, ResultStatus>
{
    public event AsyncEventHandler<ItemProcessEventArgs<T, ResultStatus>> ItemProcessed;

    private readonly IBackgroundTaskQueue _backgroundTaskQueue = backgroundTaskQueue;

    private readonly IMessageSender _processMessageSender = messageSender;   

    private readonly string _processMethodName = processMethodName;

    public async Task<ResultStatus> HandleItem(T item, CancellationToken cancellationToken = default)
    {
        await _backgroundTaskQueue.QueueBackgroundWorkItemAsync((token, logger)=>ProcessItem(item, logger, token));
            
       return ResultStatus.Success;
    }

    private async ValueTask ProcessItem(T item, ILogger logger, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_processMessageSender.IsConnected)
                await _processMessageSender.Start(cancellationToken);

            var importEntity = ConvertToImportEntity(item);

            await _processMessageSender.Send(importEntity, _processMethodName, cancellationToken);
            
            await InvokeItemProcessedAsync(item, ResultStatus.Success, cancellationToken);

            logger.LogInformation($"Item {GetUrl(item)} proccessed successfully.");
        }
        catch(Exception e)
        {
            await InvokeItemProcessedAsync(item, ResultStatus.Warning, cancellationToken);
            logger.LogError(e, $"Item {GetUrl(item)} proccessed with error.");
        }
    }

    private async Task InvokeItemProcessedAsync(T item, ResultStatus itemProcessStatus, CancellationToken cancellationToken = default)
    {
        var task = ItemProcessed?.Invoke(this, new ItemProcessEventArgs<T, ResultStatus>(item, itemProcessStatus, cancellationToken));
        if (task != null) await task;
    }

    protected abstract string GetUrl(T item);

    protected abstract TDataItem ConvertToImportEntity(T item);
}