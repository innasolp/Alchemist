using BrowserLauncher.Interfaces;
using Moq;

namespace Alchemist.BrowserService.UnitTest;

internal class BrowserLauncherMock :  Mock<IBrowserLauncher>, IBrowserLauncher
{
    public async Task<int> Close(nint handle)
    {
        return await Object.Close(handle);
    }

    public async Task Launch(string url, int milliseconds = 5000)
    {
        await Object.Launch(url, milliseconds);
    }


    async Task<nint> IBrowserLauncher.OpenUrl(string url)
    {
        return await Object.OpenUrl(url);
    }
}

internal class BrowserLauncherMock1 : BrowserLauncherMock { }
internal class BrowserLauncherMock2 : BrowserLauncherMock { }
