using Alchemist.Common;
using Alchemist.Exceptions;
using Alchemist.Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Service;

public abstract class ShopImportService(ILogger logger, ILoaderService loaderService, string host)
    : IImportService, IAsyncDisposable
{
    public abstract string Name { get; }

    protected ILoaderService LoaderService { get; } = loaderService;

    private readonly SemaphoreSlim _webLoaderSemaphoreSlim = new(1,1);

    protected ILogger Logger { get; } = logger;

    protected object? _loadData;

    private readonly string _host = host;

    public virtual async Task Start(CancellationToken stoppingToken)
    {
        bool? isStarted = null;
        while (!stoppingToken.IsCancellationRequested)
        {            
            await StartWebLoaderIfNeedAsync(stoppingToken);

            if (!LoaderService.IsStarted) break;
            else if (isStarted == null)
            {
                isStarted = true;
                Logger.LogInformation(LogMessages.ServiceStarted, Name);
            }

            await ProcessAsync(stoppingToken);

            await Task.Delay(100);
        }

        Logger.LogInformation(LogMessages.ServiceWasStopped, Name);
    }      

    protected abstract Task ProcessAsync(CancellationToken stoppingToken);

    protected virtual async Task StartWebLoaderIfNeedAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && !LoaderService.IsStarted)
        {
            try
            {
                if (!LoaderService.IsStarted)
                {
                    _loadData = await LoaderService.GetData(_host);

                    await LoaderService.Start();                    
                }
            }
            catch (WarningException warning)
            {
                Logger.LogWarning(warning, LogMessages.LoaderNotStartedWarning, [LoaderService.Name, warning.Message]);
                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogMessages.ImportWasStoppedWebLoaderNotExecute, LoaderService.Name);
                return;
            }            
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
        catch (LoaderServiceException loaderServiceEx)
        {
            await HandleLoaderServiceExceptionAsync(loaderServiceEx, url);
        }
        catch (WarningException warning)
        {
            HandleWarningException(warning, url);
        }
        catch (OperationCanceledException operationCancelledException)
        {
             await HandleCancelling(operationCancelledException, url);            
        }
        catch (Exception e)
        {
            await HandleException(e, url);            
        }
    }
        
    //todo return error or warning
    private async Task HandleLoaderServiceExceptionAsync(LoaderServiceException e, string url)
    {
        if(e.NeedAction != null )
        {
            switch (e.NeedAction)
            {
                case LoaderServiceAction.Reset:
                    Logger.LogWarning(e, LogMessages.LoadFromUrlCompletedWithErrorAndNeedReset, [url, e.Message]);
                    await ResetLoaderAsync(url);
                    await Task.Delay(100);
                    break;

                case LoaderServiceAction.Wait:
                    Logger.LogWarning(e, LogMessages.RequestFailedAndLoaderWillBePaused, [url, e.Message, 500]);
                    await Task.Delay(500);
                    break;
            }
        }
        else
        {
            Logger.LogWarning(e.InnerException ?? e, LogMessages.RequestUrlFailedWithError, [url, e.Message]);            
        }
    }


    private async Task ResetLoaderAsync(string url)
    {
        Logger.LogInformation(LogMessages.LoaderIsReseting);
        await LoaderService.Reset();
        await LoaderService.UpdateData(url);
        _loadData = await LoaderService.GetData(_host);
        Logger.LogInformation(LogMessages.LoaderResetSuccessfully);
        return;
    }

    protected virtual async Task HandleException(Exception e, string url)
    {
        Logger.LogError(e, LogMessages.ProcessUrlFailedError, url);
        await Task.FromResult(true);
    }

    protected virtual async Task HandleCancelling(OperationCanceledException operationCancelledException, string url)
    {
        if (operationCancelledException.InnerException != null)
        {
            Logger.LogError(operationCancelledException.InnerException, LogMessages.ServiceWasCancelledOnLoadingFromUrlByError,
                [Name, url, operationCancelledException.InnerException.Message]);
            await Task.FromResult(false);
        }
        else
        {
            Logger.LogInformation(LogMessages.ServiceWasCancelledOnLoadingFromUrl, Name, url);
            await Task.FromResult(true);
        }
    }

    protected virtual async Task HandleCancelling(OperationCanceledException operationCancelledException)
    {
        if (operationCancelledException.InnerException != null)
        {
            Logger.LogError(operationCancelledException.InnerException, LogMessages.ServiceWasCancelledOn,
                [Name, operationCancelledException.InnerException.Message]);
            await Task.FromResult(false);
        }
        else
        {
            Logger.LogInformation(LogMessages.ServiceWasCancelled, Name);
            await Task.FromResult(true);
        }
    }

    protected async Task<UrlTaskResult<T>> ProcessUrlTaskAsync<T>(Func<string, Task<T?>> task, string url)
    {
        try
        {
            return UrlTaskResult<T>.Success(await task(url), url);
        }
        catch (LoaderServiceException loaderServiceEx)
        {
            await HandleLoaderServiceExceptionAsync(loaderServiceEx, url);
            return await Task.FromResult(UrlTaskResult<T>.Warning(default, url, loaderServiceEx));
        }
        catch (WarningException warning)
        {
            HandleWarningException(warning, url);
            return await Task.FromResult(UrlTaskResult<T>.Warning(default, url, warning));
        }
        catch(OperationCanceledException operationCancelledException)
        {
            await HandleCancelling(operationCancelledException, url);
            return await Task.FromResult(UrlTaskResult<T>.Cancelled(url));
        }
        catch (Exception e)
        {
            await HandleException(e, url);
            return await Task.FromResult(UrlTaskResult<T>.Failed(default,url, e));
        }
    }

    protected async Task<UrlTaskResult<T>> ProcessUrlTaskAsync<TUrl, T>(Func<TUrl, Task<T?>> task, Func<TUrl, string> getUrl, TUrl itemUrl)
    {
        var url = getUrl(itemUrl);
        try
        {
            return UrlTaskResult<T>.Success(await task(itemUrl), url);
        }
        catch (LoaderServiceException loaderServiceEx)
        {
            await HandleLoaderServiceExceptionAsync(loaderServiceEx, url);
            return await Task.FromResult(UrlTaskResult<T>.Warning(default, url, loaderServiceEx));
        }        
        catch (WarningException warning)
        {
            HandleWarningException(warning, url);
            return await Task.FromResult(UrlTaskResult<T>.Warning(default,url, warning));
        }
        catch (OperationCanceledException operationCancelledException)
        {
            await HandleCancelling(operationCancelledException, url);
            return await Task.FromResult(UrlTaskResult<T>.Cancelled(url));
        }
        catch (Exception e)
        {
            await HandleException(e, url);
            return await Task.FromResult(UrlTaskResult<T>.Failed(default, url, e));
        }
    }

    protected async Task<TaskResult> ProcessTaskAsync(Func<Task> task)
    {
        try
        {
            await task();
            return TaskResult.Success();
        }
        catch (WarningException warning)
        {
            return await Task.FromResult(TaskResult.Warning(warning));
        }
        catch (OperationCanceledException operationCancelledException)
        {
            await HandleCancelling(operationCancelledException);
            return await Task.FromResult(TaskResult.Cancelled());
        }
        catch (Exception e)
        {
            return await Task.FromResult(TaskResult.Failed(e));
        }
    }

    protected async Task<TaskResult<T>> ProcessTaskAsync<T>(Func<Task<T?>> task)
    {
        try
        {
            return TaskResult<T>.Success(await task());
        }        
        catch (WarningException warning)
        {
            return await Task.FromResult(TaskResult<T>.Warning(default, warning));
        }
        catch (OperationCanceledException operationCancelledException)
        {
            await HandleCancelling(operationCancelledException);
            return await Task.FromResult(TaskResult<T>.Cancelled());
        }
        catch (Exception e)
        {
            return await Task.FromResult(TaskResult<T>.Failed(default, e));
        }
    }

    protected virtual async Task<Stream> LoadFromUrlAsync(string url)
    {
        await _webLoaderSemaphoreSlim.WaitAsync();
        try
        {
            var stream = await LoaderService.Load(url, _loadData);
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
        if (LoaderService == null)
            return;

        if (LoaderService.IsStarted)
            await LoaderService.Close();

        await LoaderService.DisposeAsync();
    }
}
