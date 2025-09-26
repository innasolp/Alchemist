using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Model;

public abstract class ModelJsonBinder(ILogger logger) : ModelBinder(logger)
{
    protected abstract Type ModelType { get; }

    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.General)
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNameCaseInsensitive = true
    };

    protected override object? GetData(ModelBindingContext bindingContext)
    {
        var model = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (model.Values.Count == 0)        
            throw new InvalidDataException($"Model {bindingContext.ModelName} does not contains values");        

        return JsonSerializer.Deserialize(model.FirstValue, ModelType, DefaultJsonSerializerOptions);        
    }
}
