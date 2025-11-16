using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alchemist.Web.ModelBinder;

public class ModelImplementationJsonBinder(ILogger<ModelImplementationJsonBinder> logger) : IModelBinder
{
    private readonly ILogger<ModelImplementationJsonBinder> _logger = logger;

    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.General)
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNameCaseInsensitive = true
    };

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        if (!typeof(IModel).IsAssignableFrom(bindingContext.ModelType))
        {
            throw new NotSupportedException($"The '{nameof(ModelImplementationJsonBinder)}' model binder should only be used on {typeof(IModel).Name}, it will not work on '{bindingContext.ModelType.Name}'");
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
            if(model.Values.Count == 0 )
            {
                _logger.LogError($"Model {bindingContext.ModelName} does not contains values");
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
