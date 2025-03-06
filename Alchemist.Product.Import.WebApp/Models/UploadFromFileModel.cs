namespace Alchemist.Product.Import.WebApp.Models;

public class UploadFromFileModel
{
    public const string onSuccess = "$(#onsuccess)";
     
    public string Id { get; set; }

    public string OnChange { get; set; }

    public string ButtonLabel { get; set; } = "From file";

    public string LabelId => $"{Id}_label";

    private string OnSetFileName => $"$('#{LabelId}').html('<i>'+ this.files[0].name + '</i>');";

    public string OnChangeWithFileNameLabel => OnChange.Contains(onSuccess) ? OnChange.Replace(onSuccess, OnSetFileName) : OnChange;

    public string? FileName { get; set; }
    
}
