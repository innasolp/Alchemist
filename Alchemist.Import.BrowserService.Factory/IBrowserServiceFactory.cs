using Alchemist.Import.Interfaces;

namespace Alchemist.Import.BrowserService.Factory;

public interface IBrowserServiceFactory
{
    IBrowserService Create(string browserLoader, string browserLauncher);
}
