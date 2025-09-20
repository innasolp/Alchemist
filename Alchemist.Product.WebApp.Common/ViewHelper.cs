using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.WebApp.Common;

public static class ViewHelper
{
    public static async Task<string> GetViewHtml(IServiceProvider serviceProvider, ActionContext actionContext, string viewName, object? model = null)
    {
        var viewEngine = serviceProvider.GetRequiredService<IRazorViewEngine>();
        var viewResult = viewEngine.GetView("", viewName, isMainPage: false);

        if (!viewResult.Success)
        {
            throw new InvalidOperationException($"Could not find view '{viewName}'");
        }

        var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), actionContext.ModelState)
        {
            Model = model
        };

        using var sw = new StringWriter();

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewData,
            new TempDataDictionary(actionContext.HttpContext, serviceProvider.GetRequiredService<ITempDataProvider>()),
            sw,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);
        return sw.ToString();
    }
}
