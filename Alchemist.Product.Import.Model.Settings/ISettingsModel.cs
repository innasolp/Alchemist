using Alchemist.Web.ModelBinder.Alchemist.Product.Model;

namespace Alchemist.Product.Import.Model;

public interface ISettingsModel : IModel
{
    Guid ShopGuid { get; }    
}
