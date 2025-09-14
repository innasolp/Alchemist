using Alchemist.Common;
using Alchemist.Exceptions;
using Alchemist.Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Service;

public abstract class ImportService(ILogger logger, ILoaderService loaderService, string host)
    : IImportService, IAsyncDisposable
{
    public abstract string Name { get; }

    protected ILoaderService LoaderService { get; } = loaderService;

    protected ILogger Logger { get; } = logger;

    private bool? _isStarted = null;

    public bool IsStarted => _isStarted == true;

    protected object? _loadData;

    private readonly string _host = host;

    private readonly SemaphoreSlim _resetSemaphorSlim = new(1, 1);

    private bool _loaderIsReseted = false;

    public async Task Start(CancellationToken stoppingToken)
    {
        var innerTokenSource = new CancellationTokenSource();

        await StartAsync(stoppingToken, innerTokenSource);
    }

    protected virtual async Task StartAsync(CancellationToken stoppingToken, CancellationTokenSource serviceToken)
    {
        var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, serviceToken.Token);

        _loaderIsReseted = false;

        while (!tokenSource.IsCancellationRequested)
        {
            await StartWebLoaderIfNeedAsync(tokenSource.Token);

            if (!LoaderService.IsStarted) break;
            else if (_isStarted == null)
            {
                _isStarted = true;
                _loadData = await LoaderService.GetData(_host);
                Logger.LogInformation(LogMessages.ServiceStarted, Name);
            }

            try
            {
                await ProcessAsync(stoppingToken, serviceToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                _isStarted = false;
                throw;
            }
            finally
            {
                await Task.Delay(100);
            }
        }

        Logger.LogInformation(LogMessages.ServiceWasStopped, Name);
    }

    protected abstract Task ProcessAsync(CancellationToken stoppingToken, CancellationTokenSource serviceToken);

    protected virtual async Task StartWebLoaderIfNeedAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && !LoaderService.IsStarted)
        {
            try
            {
                if (!LoaderService.IsStarted)
                    await LoaderService.Start();
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
            await HandleCancellingAsync(operationCancelledException, url);
        }
        catch (Exception e) when (e is not ImportCanceledException)
        {
            await HandleExceptionAsync(e, url);
        }
    }

    private async Task<ResultStatus> HandleLoaderServiceExceptionAsync(LoaderServiceException e, string url)
    {
        if (e.NeedAction != null)
        {
            switch (e.NeedAction)
            {
                case LoaderServiceAction.Reset:
                    return await HandleResetingAsync(e, url);

                case LoaderServiceAction.Wait:
                    Logger.LogWarning(e, LogMessages.RequestFailedAndLoaderWillBePaused, [url, e.Message, 500]);
                    await Task.Delay(500);
                    return await Task.FromResult(ResultStatus.Warning);

                default:
                    Logger.LogWarning(e, LogMessages.RequestUrlFailedWithError, [url, e.Message]);
                    return await Task.FromResult(ResultStatus.Warning);
            }
        }
        else
        {
            Logger.LogWarning(e.InnerException ?? e, LogMessages.RequestUrlFailedWithError, [url, e.Message]);
            return await Task.FromResult(ResultStatus.Warning);
        }
    }

    private async Task<ResultStatus> HandleResetingAsync(LoaderServiceException e, string url)
    {
        if (_loaderIsReseted)
        {
            Logger.LogError(e, LogMessages.ImportWasStoppedLoaderServiceAlreadyReseted, [LoaderService.Name, Name]);
            var message = string.Format(Messages.ImportWasCancelledOnUrlBecauseLoaderFailed, [Name, url, LoaderService.Name]);
            throw new ImportCanceledException(message, e);
        }

        Logger.LogWarning(e, LogMessages.LoadFromUrlCompletedWithErrorAndNeedReset, [url, e.Message]);

        try
        {
            await ResetLoaderAsync(url);

            Logger.LogInformation(LogMessages.LoaderResetSuccessfully);

            return await Task.FromResult(ResultStatus.Warning);
        }
        catch (Exception resetException)
        {
            Logger.LogError(resetException, LogMessages.LoaderServiceResetingFailed,
                [LoaderService.Name, resetException.Message]);

            return await Task.FromResult(ResultStatus.Error);
        }
        finally
        {
            await Task.Delay(100);
        }
    }

    private async Task ResetLoaderAsync(string url)
    {
        try
        {
            await _resetSemaphorSlim.WaitAsync();

            Logger.LogInformation(LogMessages.LoaderIsReseting);
            await LoaderService.Reset();
            await LoaderService.UpdateData(url);
            _loadData = await LoaderService.GetData(_host);
            _loaderIsReseted = true;
        }
        finally
        {
            _resetSemaphorSlim.Release();
        }
    }

    protected virtual async Task HandleExceptionAsync(Exception e, string url)
    {
        Logger.LogError(e, LogMessages.ProcessUrlFailedError, url);
        await Task.FromResult(true);
    }

    protected virtual async Task HandleCancellingAsync(OperationCanceledException operationCancelledException, string url)
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
            throw operationCancelledException;
        }
    }

    protected virtual async Task HandleCancellingAsync(OperationCanceledException operationCancelledException)
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
            throw operationCancelledException;
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
            var resultStatus = await HandleLoaderServiceExceptionAsync(loaderServiceEx, url);
            return await Task.FromResult(UrlTaskResult<T>.FromStatus(resultStatus, url, loaderServiceEx));
        }
        catch (WarningException warning)
        {
            HandleWarningException(warning, url);
            return await Task.FromResult(UrlTaskResult<T>.Warning(default, url, warning));
        }
        catch (OperationCanceledException operationCancelledException)
        {
            await HandleCancellingAsync(operationCancelledException, url);
            return await Task.FromResult(UrlTaskResult<T>.Cancelled(url));
        }
        catch (Exception e) when (e is not ImportCanceledException)
        {
            await HandleExceptionAsync(e, url);
            return await Task.FromResult(UrlTaskResult<T>.Failed(default, url, e));
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
            var resultStatus = await HandleLoaderServiceExceptionAsync(loaderServiceEx, url);
            return await Task.FromResult(UrlTaskResult<T>.FromStatus(resultStatus, url, loaderServiceEx));
        }
        catch (WarningException warning)
        {
            HandleWarningException(warning, url);
            return await Task.FromResult(UrlTaskResult<T>.Warning(default, url, warning));
        }
        catch (OperationCanceledException operationCancelledException)
        {
            await HandleCancellingAsync(operationCancelledException, url);
            return await Task.FromResult(UrlTaskResult<T>.Cancelled(url));
        }
        catch (Exception e) when (e is not ImportCanceledException)
        {
            await HandleExceptionAsync(e, url);
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
            await HandleCancellingAsync(operationCancelledException);
            return await Task.FromResult(TaskResult.Cancelled());
        }
        catch (Exception e) when (e is not ImportCanceledException)
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
            await HandleCancellingAsync(operationCancelledException);
            return await Task.FromResult(TaskResult<T>.Cancelled());
        }
        catch (Exception e)
        {
            return await Task.FromResult(TaskResult<T>.Failed(default, e));
        }
    }

    protected virtual async Task<Stream> LoadFromUrlAsync(string url)
    {
        var stream = await LoaderService.Load(url, _loadData);
        return await Task.FromResult(stream);
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
