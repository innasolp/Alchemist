using Alchemist.Product.Import.Model;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace Alchemist.Product.Import.WebApp.Models;

public class ModelImplementationJsonEnumerableBinder(ILogger<ModelImplementationJsonEnumerableBinder> logger) : IModelBinder
{
    private readonly ILogger<ModelImplementationJsonEnumerableBinder> _logger = logger;

    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.General)
    {
        IgnoreReadOnlyProperties = false,
        IgnoreReadOnlyFields = true,
        RespectRequiredConstructorParameters = true    ,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
        PropertyNameCaseInsensitive = true,        
    };

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        if (!typeof(IEnumerable<IModel>).IsAssignableFrom(bindingContext.ModelType))
        {
            throw new NotSupportedException($"The '{nameof(ModelImplementationJsonBinder)}' model binder should only be used on {typeof(IEnumerable<IModel>).Name }, it will not work on '{bindingContext.ModelType.Name}'");
        }

        try
        {
            if(string.IsNullOrEmpty(bindingContext.ModelName))
            {
                _logger.LogWarning($"ModelName of  {bindingContext.ModelType.Name} is empty");
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            var model = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (model.Values.Count == 0)
            {
                _logger.LogError($"Model {bindingContext.ModelName} does not contaons values");
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            var data = JsonSerializer.Deserialize(model.FirstValue, bindingContext.ModelType, DefaultJsonSerializerOptions);

            bindingContext.Result = ModelBindingResult.Success(data);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error when trying to model bind {bindingContext.ModelType.Name}");
            bindingContext.Result = ModelBindingResult.Failed();
        }
    }
}
