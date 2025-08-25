using Alchemist.Import.Interfaces;

namespace Alchemist.Import.Factory.BrowserService;

public interface IBrowserServiceFactory
{
    IBrowserService Create(string browserLoader, string browserLauncher);
}
