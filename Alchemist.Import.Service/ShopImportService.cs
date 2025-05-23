using Alchemist.Common;
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

    protected IWebLoader WebLoader { get; } = webLoader;

    protected RequestHeaders? RequestHeaders { get; } = requestHeaders;

    protected ILogger Logger { get; } = logger;

    public abstract Task Start(CancellationTokenSource stoppingToken);

    protected virtual async Task StartWebLoaderIfNeedAsync(CancellationTokenSource stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && !WebLoader.IsStarted)
        {
            try
            {
                if (!WebLoader.IsStarted)
                    await WebLoader.Start();
            }
            catch (WarningException warning)
            {
                Logger.LogWarning(warning, LogMessages.WebLoaderNotStartedWarning, [WebLoader.GetType().Name, warning.Message]);
                await Task.Delay(1000, stoppingToken.Token);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogMessages.ImportWasStoppedWebLoaderNotExecute, WebLoader.GetType().Name);
                return;
            }            
        }
    }

    protected virtual async Task HandleWebLoaderExceptionAsync(WebLoaderException wle, string url)
    {
        switch (wle.NsError)
        {
            case NsError.NS_ERROR_REDIRECT_LOOP:
                if (WebLoader.IsStarted)
                {
                    Logger.LogWarning(wle, LogMessages.WebLoaderThrowsNsRedirectLoopAndWillBeReseted, url);
                    Logger.LogInformation(LogMessages.WebLoaderIsReseting);
                    await WebLoader.Reset();
                    await Task.Delay(1000);
                    Logger.LogInformation(LogMessages.WebLoaderResetSuccessfully);
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
            Logger.LogWarning(e, LogMessages.TooManyRequestsError);
            await Task.Delay(10000);
        }
        else if (RequestHeaders != null &&
            (e.StatusCode == System.Net.HttpStatusCode.Forbidden || e.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable))
        {
            Logger.LogWarning(e, LogMessages.HttpRequestErrorAndWebLoaderRestart, [e.StatusCode, url, WebLoader.GetType().Name]);
            await Task.Delay(500);
            await WebLoader.Start();
        }
        else
        {
            Logger.LogError(e, LogMessages.RequestUrlFailedWithErrorAndStatusCode, [url, e.HttpRequestError, e.StatusCode]);
        }
    }

    protected virtual void HandleWarningException(WarningException warning, string url)
    {
        Logger.LogWarning(warning, LogMessages.ProcessUrlNotCompleteWarning, [url, warning.Message]);
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
            await HandleWebLoaderExceptionAsync(wle, url);
        }
        catch (WarningException warning)
        {
            HandleWarningException(warning, url);
        }
        catch (Exception e)
        {
            Logger.LogError(e, LogMessages.ProcessUrlFailedError, url);
        }
    }

    protected async Task<TaskResult<T>> ProcessUrlTaskAsync<T>(Func<string, Task<T?>> task, string url)
    {
        try
        {
            return TaskResult<T>.Success(await task(url));
        }
        catch (HttpRequestException e)
        {
            await HandleHttpExceptionAsync(e, url);
            return await Task.FromResult(TaskResult<T>.Warning(default, e));
        }
        catch (WebLoaderException wle)
        {
            await HandleWebLoaderExceptionAsync(wle, url);
            return await Task.FromResult(TaskResult<T>.Warning(default, wle));
        }
        catch (WarningException warning)
        {
            HandleWarningException(warning, url);
            return await Task.FromResult(TaskResult<T>.Warning(default, warning));
        }
        catch (Exception e)
        {
            Logger.LogError(e, LogMessages.ProcessUrlFailedError, url);
            return await Task.FromResult(TaskResult<T>.Failed(default, e));
        }
    }

    protected async Task<TaskResult<T>> ProcessUrlTaskAsync<TUrl, T>(Func<TUrl, Task<T?>> task, Func<TUrl, string> getUrl, TUrl itemUrl)
    {
        try
        {
            return TaskResult<T>.Success(await task(itemUrl));
        }
        catch (HttpRequestException e)
        {
            await HandleHttpExceptionAsync(e, getUrl(itemUrl));
            return await Task.FromResult(TaskResult<T>.Warning(default, e));
        }
        catch (WebLoaderException wle)
        {
            await HandleWebLoaderExceptionAsync(wle, getUrl(itemUrl));
            return await Task.FromResult(TaskResult<T>.Warning(default, wle));
        }
        catch (WarningException warning)
        {
            HandleWarningException(warning, getUrl(itemUrl));
            return await Task.FromResult(TaskResult<T>.Warning(default, warning));
        }
        catch (Exception e)
        {
            Logger.LogError(e, LogMessages.ProcessUrlFailedError, getUrl(itemUrl));
            return await Task.FromResult(TaskResult<T>.Failed(default, e));
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
