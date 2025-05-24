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

    private readonly SemaphoreSlim _webLoaderSemaphoreSlim = new(1,1);

    protected RequestHeaders? RequestHeaders { get; } = requestHeaders;

    protected ILogger Logger { get; } = logger;

    public virtual async Task Start(CancellationToken stoppingToken)
    {
        bool? isStarted = null;
        while (!await ExecutingCancellationNeeded(stoppingToken))
        {
            await StartWebLoaderIfNeedAsync(stoppingToken);

            if (!WebLoader.IsStarted) break;
            else if (isStarted == null)
            {
                isStarted = true;
                Logger.LogInformation(LogMessages.ServiceStarted, Name);
            }

            await ProcessAsync(stoppingToken);
        }

        Logger.LogInformation(LogMessages.ServiceWasStopped, Name);
    }

    protected virtual async Task<bool> ExecutingCancellationNeeded(CancellationToken stoppingToken)
    {
        return await Task.FromResult(stoppingToken.IsCancellationRequested);
    }

    protected abstract Task ProcessAsync(CancellationToken stoppingToken);

    protected virtual async Task StartWebLoaderIfNeedAsync(CancellationToken stoppingToken)
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
                await Task.Delay(1000, stoppingToken);
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
            await HandleException(e, url);            
        }
    }

    protected virtual async Task HandleException(Exception e, string url)
    {
        Logger.LogError(e, LogMessages.ProcessUrlFailedError, url);
        await Task.FromResult(true);
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
            await HandleException(e, url);
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
            await HandleException(e, getUrl(itemUrl));
            return await Task.FromResult(TaskResult<T>.Failed(default, e));
        }
    }

    protected virtual async Task<Stream> LoadFromUrlAsync(string url)
    {
        await _webLoaderSemaphoreSlim.WaitAsync();
        try
        {
            var stream = await WebLoader.LoadFromUrl(url, RequestHeaders);
            return await Task.FromResult(stream);
        }
        catch { throw; }
        finally
        {
            _webLoaderSemaphoreSlim.Release();
        }        
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (WebLoader == null)
            return;

        if (WebLoader.IsStarted)
            await WebLoader.Close();

        await WebLoader.DisposeAsync();
    }
}
