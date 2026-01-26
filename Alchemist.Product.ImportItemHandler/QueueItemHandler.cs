using BackgroundTaskQueue;
using Import.Interfaces;
using Message.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Product.ImportItemHandler;

public abstract class QueueItemHandler<T, TDataItem>(IBackgroundTaskQueue backgroundTaskQueue, 
    IMessageSender messageSender, 
    string processMethodName) 
    : Import.IItemHandler<T, ResultStatus>
{
    public event Import.AsyncEventHandler<Import.ItemProcessEventArgs<T, ResultStatus>> ItemProcessed;

    private readonly IBackgroundTaskQueue _backgroundTaskQueue = backgroundTaskQueue;

    private readonly IMessageSender _processMessageSender = messageSender;   

    private readonly string _processMethodName = processMethodName;

    public async Task<ResultStatus> HandleItem(T item, CancellationToken cancellationToken = default)
    {
        await _backgroundTaskQueue.QueueBackgroundWorkItemAsync((token, logger)=>ProcessItem(item, logger, token), cancellationToken);
            
       return ResultStatus.Success;
    }

    private async ValueTask ProcessItem(T item, ILogger logger, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_processMessageSender.IsConnected)
                await _processMessageSender.Start(cancellationToken);

            var importEntity = await ConvertToImportEntity(item);

            logger.LogInformation($"Item {GetUrl(item)} converted successfully.");

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
        var task = ItemProcessed?.Invoke(this, new Import.ItemProcessEventArgs<T, ResultStatus>(item, itemProcessStatus, cancellationToken));
        if (task != null) await task;
    }

    protected abstract string GetUrl(T item);

    protected abstract Task<TDataItem> ConvertToImportEntity(T item);
}