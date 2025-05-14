using Alchemist.Import.Interfaces;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.Import.Service;

public abstract class ShopImportService(ILogger logger, IWebLoader webLoader, RequestHeaders? requestHeaders)
    : IImportService, IAsyncDisposable
{
    public abstract string Name { get; }

    public IWebLoader WebLoader { get; } = webLoader;

    private readonly RequestHeaders? _requestHeaders = requestHeaders;

    protected ILogger Logger { get; } = logger;

    public abstract Task Start(CancellationToken stoppingToken);

    protected virtual async Task StartWebLoaderIfNeedAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && !WebLoader.IsStarted)
        {
            try
            {
                if (!WebLoader.IsStarted)
                    await WebLoader.Start(_requestHeaders);
            }
            catch (WarningException warning)
            {
                Logger.LogWarning(warning, $"importer {WebLoader.GetType()} not started.Warning : {warning.Message}. ");
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"importer {WebLoader.GetType()} was not executed. Import is stopped");
                return;
            }
            await Task.Delay(1000, stoppingToken);
        }
    }

    protected virtual async Task HandleWebLoaderExceptionAsync(WebLoaderException wle)
    {
        switch (wle.NsError)
        {
            case NsError.NS_ERROR_REDIRECT_LOOP:
                if (WebLoader.IsStarted)
                {
                    Logger.LogWarning($"Web loader will be reset. {wle.Message}");
                    Logger.LogInformation("Web loader is reseting...");
                    await WebLoader.Reset();
                    await Task.Delay(30000);
                    Logger.LogInformation("Web loader reset successfully.");
                }
                return;


            default:
                throw wle;
        }
    }

    protected virtual async Task HandleHttpExceptionAsync(HttpRequestException e, string url)
    {
        if (e.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            Logger.LogWarning("Too many request. Thread would be sleeped 10 sec");
            await Task.Delay(10000);
        }
        else if (_requestHeaders != null &&
            (e.StatusCode == System.Net.HttpStatusCode.Forbidden || e.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable))
        {
            Logger.LogWarning($"Response status {e.StatusCode} for url {url}. Web loader {WebLoader.GetType().Name} will be restarted.");
            await WebLoader.Start(_requestHeaders);
        }
        else
        {
            Logger.LogError(e, $"request url {url} failed with error {e.HttpRequestError} status {e.StatusCode}");
        }
    }

    protected virtual void HandleWarningException(WarningException warning, string url)
    {
        Logger.LogWarning(warning, $"process url {url} not complete. Warning : {warning.Message}.");
    }

    protected async Task ProcessUrlTaskAsync(Func<string, Task> task, string url)
    {
        try
        {
            await task(url);
        }
        catch (HttpRequestException e)
        {
            await HandleHttpExceptionAsync(e, url);
        }
        catch (WebLoaderException wle)
        {
            await HandleWebLoaderExceptionAsync(wle);
        }
        catch (WarningException warning)
        {
            HandleWarningException(warning, url);    
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"process {url} failed");
        }
    }

    protected async Task<T?> ProcessUrlTaskAsync<T>(Func<string, Task<T?>> task, string url)
    {
        try
        {
            return await task(url);
        }
        catch (HttpRequestException e)
        {
            await HandleHttpExceptionAsync(e, url);
            return await Task.FromResult(default(T));
        }
        catch (WebLoaderException wle)
        {
            await HandleWebLoaderExceptionAsync(wle);
            return await Task.FromResult(default(T));
        }
        catch (WarningException warning)
        {
            HandleWarningException(warning, url);
            return await Task.FromResult(default(T));
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"process {url} failed");
            return await Task.FromResult(default(T));
        }
    }

    protected async Task<T?> ProcessUrlTaskAsync<TUrl, T>(Func<TUrl, Task<T?>> task, Func<TUrl, string> getUrl, TUrl itemUrl)
    {
        try
        {
            return await task(itemUrl);
        }
        catch (HttpRequestException e)
        {
            await HandleHttpExceptionAsync(e, getUrl(itemUrl));
            return await Task.FromResult(default(T));
        }
        catch (WebLoaderException wle)
        {
            await HandleWebLoaderExceptionAsync(wle);
            return await Task.FromResult(default(T));
        }
        catch (WarningException warning)
        {
            HandleWarningException(warning, getUrl(itemUrl));
            return await Task.FromResult(default(T));
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"process {getUrl(itemUrl)} failed");
            return await Task.FromResult(default(T));
        }
    }


    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (WebLoader == null)
            return;

        if (WebLoader.IsStarted)
            await WebLoader.Close();

        await WebLoader.DisposeAsync();
    }
}
