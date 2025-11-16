using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;


namespace Alchemist.Web.Components.TagHelpers;

[HtmlTargetElement("input", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
public class ModelInputTagHelper(IHtmlGenerator generator, IModelMetadataProvider metadataProvider) : InputTagHelper(generator)
{
    private const string ForAttributeName = "asp-for-model";

    private readonly IModelMetadataProvider _metadataProvider = metadataProvider;
    
    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression ForModel { get; set; }

    public string ModelProperty { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrEmpty(ModelProperty))
            return;

        For = ForModel.GetModelExpressionForProperty(_metadataProvider, ModelProperty);

        base.Process(context, output);
    }
}
