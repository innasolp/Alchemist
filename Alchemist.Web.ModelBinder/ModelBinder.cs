using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace Alchemist.Web.ModelBinder;

public abstract class ModelBinder(ILogger logger) : IModelBinder
{
    protected ILogger Logger { get; } = logger;    

    protected abstract object? GetData(ModelBindingContext bindingContext);

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        try
        {
            if (string.IsNullOrEmpty(bindingContext.ModelName))            
                throw new InvalidDataException($"ModelName of  {bindingContext.ModelType.Name} is empty");
            
            var data = GetData(bindingContext);

            bindingContext.Result = ModelBindingResult.Success(data);
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"Error when trying to model bind {bindingContext.ModelType.Name}");
            bindingContext.Result = ModelBindingResult.Failed();
        }
    }
}
