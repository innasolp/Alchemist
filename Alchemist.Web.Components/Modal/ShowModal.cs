using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace Alchemist.Web.Components.Modal;

public class ShowModal : ViewComponent
{ 
    public IViewComponentResult Invoke(string modalDivClass = "modalDiv", string modalBodyDivClass = "modalBody", string modalCloseBtn = "modalCloseBtn")
    {
        var html = ComponentHtmlHelper.GetHtml("Alchemist.Web.Components.Modal.ShowModal.html");
        var content = string.Format(html, [modalDivClass, modalBodyDivClass, modalCloseBtn]);
        return new HtmlContentViewComponentResult(new HtmlString(content));
    }
}
