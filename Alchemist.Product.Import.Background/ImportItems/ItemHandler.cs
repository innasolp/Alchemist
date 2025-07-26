using Alchemist.Common;
using Alchemist.Exceptions;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Message.Interfaces;

namespace Alchemist.Product.Import.Background.ImportItems;

internal abstract class ItemHandler<T, TEntity>(IMessageSender messageSender, string methodName) : IItemHandler<T>
    where T: IItem
{
    private readonly IMessageSender _messageSender = messageSender;

    private readonly string _methodName = methodName;

    public event AsyncItemHandler<T> ItemProcessed;
    public async Task<ResultStatus> HandleItem(T item, IShopModel shopModel)
    {
        var importEntity = ConvertToImportEntity(item);

        try
        {
            await _messageSender.Send(importEntity, _methodName);
            await InvokeItemProcessedAsync(item, shopModel, ResultStatus.Success);
            return ResultStatus.Success;
        }
        catch (Exception e)
        {
            await InvokeItemProcessedAsync(item, shopModel, ResultStatus.Warning);
            throw new WarningException($"Item {item.Url} proccessed with error.", e);
        }
    }

    protected abstract TEntity ConvertToImportEntity(T item);

    private Task InvokeItemProcessedAsync(T item, IShopModel shopModel, ResultStatus itemProcessStatus)
    {
        return ItemProcessed?.Invoke(this, item, shopModel, itemProcessStatus) ?? Task.FromResult(false);
    }

    public async Task<ResultStatus> HandleItem(IItem item, IShopModel shopModel)
    {
        if (item is not T productItem)
            throw new InvalidDataException($"Invalid item type {item.GetType().Name}. Must be {nameof(IProductItem)}.");

        return await HandleItem(productItem, shopModel);
    }

}
