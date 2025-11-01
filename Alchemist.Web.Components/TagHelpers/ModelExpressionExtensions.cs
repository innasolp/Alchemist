using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Alchemist.Web.Components.TagHelpers;

internal static class ModelExpressionExtensions
{
    public static ModelExpression GetParentModelExpression(this ModelExpression modelExpression, IModelMetadataProvider metadataProvider, string parentPropertyName, string targetPropertyName )
    {
        var parentProperty = modelExpression.Metadata.Properties[parentPropertyName]
            ?? throw new InvalidOperationException($"Model metadata {modelExpression.Metadata.ContainerType} does not contains property {parentPropertyName}");

        var parentPropertyMetaData = parentProperty.Properties[targetPropertyName]
            ??
             throw new InvalidOperationException($"parent model metadata {parentProperty.ContainerType} does not contains property {targetPropertyName}");

        var parentModelExplorer = modelExpression.ModelExplorer.Properties.ToList().FirstOrDefault(pm => pm.Metadata.PropertyName == parentPropertyName)
            ?? throw new InvalidOperationException($"model explorer {modelExpression.ModelExplorer.ModelType} does not contains property {parentPropertyName}.");

        return new ModelExpression(targetPropertyName, new ModelExplorer(metadataProvider, parentPropertyMetaData, parentModelExplorer.Model));
    }

    public static ModelExpression GetModelExpressionForProperty(this ModelExpression modelExpression, IModelMetadataProvider metadataProvider, string targetPropertyName )
    {
        var targetPropertyMetaData = modelExpression.Metadata.Properties[targetPropertyName]
            ??
             throw new InvalidOperationException($"parent model metadata {modelExpression.Metadata.ContainerType} does not contains property {targetPropertyName}");

        return new ModelExpression(targetPropertyName, new ModelExplorer(metadataProvider, targetPropertyMetaData, modelExpression.ModelExplorer.Model));
    }
}
