using Alchemist.Product.Import.Model.Infrastructure;

namespace Alchemist.Product.Import.Model;

public interface ITabModel : ISettingsModel
{
    TabType Tab { get; }
}
