using Alchemist.Import.Products.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.ImportItemHandler;
using BackgroundTaskQueue;
using Json.CustomSerialization;
using Message.Interfaces;
using System.Text.Json;

namespace Alchemist.Product.BeautyAndHealth.ImportItemHandler;

internal class ProductQueueItemHandler(IBackgroundTaskQueue backgroundTaskQueue, IMessageSender messageSender, string methodName, IJsonSettings jsonSettings) 
    : QueueItemHandler<IImportProduct, ImportBeautyAndHealthProductCommand>(backgroundTaskQueue, messageSender, methodName), IProductItemHandler
{
    private readonly JsonLoader _jsonLoader = new(jsonSettings);    

    protected override async Task<ImportBeautyAndHealthProductCommand> ConvertToImportEntity(IImportProduct item)
    {
        return new ImportBeautyAndHealthProductCommand(await ConvertToImportProductItem(item.ProductItem, item.SourceName, item.SourcePath, item.Stream));
    }

    protected override string GetUrl(IImportProduct item)
    {
        return item.ProductItem.AbsolutePath;
    }

    private async Task<BeautyAndHealthProductData> ConvertToImportProductItem(IProductItem productItem, string shopName, string shopUrl, Stream stream)
    {
        var json = await JsonSerializer.DeserializeAsync<System.Text.Json.Nodes.JsonObject>(stream);
        var beautyAndHealthProduct = new BeautyAndHealthProduct();
        _jsonLoader.LoadFromJson(beautyAndHealthProduct, json);

        return new BeautyAndHealthProductData
        {
            Product = new Entities.Product { Name = productItem.Name, Articul = beautyAndHealthProduct.Articul },
            ShopProduct = new ShopProduct { ApiUrl = productItem.AbsolutePath, ItemUrl = productItem.ItemId, ItemId = productItem.ItemId },
            ShopProductPrice = new ShopProductPrice { Price = productItem.Price },
            Brand = !string.IsNullOrEmpty(productItem.Brand) ? new Brand { Name = productItem.Brand } : null,
            Country = !string.IsNullOrEmpty(beautyAndHealthProduct.Country) ? new Country { Name = beautyAndHealthProduct.Country } : null,
            Currency = new Currency { Name = productItem.Currency },
            ProductType = new ProductType { Name = beautyAndHealthProduct.ProductType },
            PurposeTypes = [.. beautyAndHealthProduct.Purposes?.Select(p => new PurposeType { Name = p }) ?? []], 
            Components = [.. beautyAndHealthProduct.Components?.Select(c => new Component { Name = c }) ?? []],
            ShopCategory = new ShopCategory { ItemId = productItem.CategoryId },
            ShopName = shopName,
            ShopUrl = shopUrl,
        };
    }
}