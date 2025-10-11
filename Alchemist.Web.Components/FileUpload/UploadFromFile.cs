using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace Alchemist.Web.Components.FileUpload;

public class UploadFromFile : ViewComponent
{
    private const string OnSetFileNameFormat = "$('#{0}').html('<i>'+ this.files[0].name + '</i>');";

    public IViewComponentResult Invoke(string onChange, string id = "uploadFromFile", string buttonLabel = "From file", string? fileName = null, bool setFileName = true)
    {
        var labelId = $"{id}_label";
        var onChangeWithFileNameLabel = onChange;
        if(setFileName)
        {
            var onSetFileName = string.Format(OnSetFileNameFormat, labelId);

            if (onChange.Contains("}"))
                onChangeWithFileNameLabel = onChangeWithFileNameLabel.Replace("}", $";{onSetFileName}}}");
            else
                onChangeWithFileNameLabel = $"{onChange} ; {onSetFileName}";
        }        

        var html = ComponentHtmlHelper.GetHtml("Alchemist.Web.Components.FileUpload.FileUpload.html");
        var content = string.Format(html, [id, labelId, fileName, onChangeWithFileNameLabel, buttonLabel]);
        return new HtmlContentViewComponentResult(new HtmlString(content));
    }
}
