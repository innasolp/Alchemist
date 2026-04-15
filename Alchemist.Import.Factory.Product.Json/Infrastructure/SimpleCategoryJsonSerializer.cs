using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using System.Text.Json;

namespace Alchemist.Import.Factory.Product.Json.Infrastructure;

public class SimpleCategoryJsonSerializer<TCategory> : IItemSerializer<TCategory>
    where TCategory : class, ICategoryProducts, new()
{
    public async Task<TCategory?> DeserializeFromStream(Stream stream, CancellationToken cancellationToken = default)
    {
        return await JsonSerializer.DeserializeAsync<TCategory>(stream, cancellationToken: cancellationToken);
    }
}