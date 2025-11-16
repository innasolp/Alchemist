using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alchemist.Web.Components.TagHelpers;

[HtmlTargetElement("span", Attributes = ValidationForAttributeName)]

public class ModelValidationMessageTagHelper(IHtmlGenerator generator, IModelMetadataProvider metadataProvider) : ValidationMessageTagHelper(generator)
{
    private const string ValidationForAttributeName = "asp-validation-for-model";

    private readonly IModelMetadataProvider _metadataProvider = metadataProvider;

    [HtmlAttributeName(ValidationForAttributeName)]
    public ModelExpression ForModel { get; set; }

    public string ModelProperty { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrEmpty(ModelProperty))
            return;

        For = ForModel.GetModelExpressionForProperty(_metadataProvider, ModelProperty);

        await base.ProcessAsync(context, output);
    }
}
