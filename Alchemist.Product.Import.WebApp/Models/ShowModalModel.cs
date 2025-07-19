namespace Alchemist.Product.Import.WebApp.Models;

public class ShowModalModel
{
    public required string ModalDivClass { get; set; } 

    public required string ModalBodyDivClass { get; set; }

    public string ModalCloseBtn { get; set; } = "#modalCloseBtn";
}
