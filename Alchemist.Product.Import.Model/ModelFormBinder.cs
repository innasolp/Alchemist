using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace Alchemist.Product.Model;

public abstract class ModelFormBinder(ILogger logger) : ModelBinder(logger)
{
    protected abstract string[] PropertyNames { get; }

    protected override object? GetData(ModelBindingContext bindingContext)
    {
        if (bindingContext.BindingSource?.Id != "Form")
            throw new InvalidOperationException($"BindingSource of  {bindingContext.ModelType.Name} is {bindingContext.BindingSource?.Id ?? "null"}");

        Dictionary<string, string?> propertyValues = [];
        foreach (var property in PropertyNames)
        {
            var valueResult = bindingContext.ValueProvider.GetValue(property);
            propertyValues.Add(property, valueResult.FirstValue);
        }

        return GetValueByProperties(propertyValues);
    }

    protected abstract object? GetValueByProperties(Dictionary<string, string?> propertyValues);
}
