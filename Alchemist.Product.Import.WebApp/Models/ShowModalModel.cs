namespace Alchemist.Product.Import.WebApp.Models;

public class ShowModalModel(string modalDivClass, string modalBodyDivClass)
{
    public string ModalDivClass { get; } = modalDivClass;
    public string ModalBodyDivClass { get; } = modalBodyDivClass;
}
