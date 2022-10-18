using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks.Dataflow;

namespace WebHooks.WebApp.Model;

public class DataflowWebHookSender : WebHookSender
{
    private const int DefaultMaxConcurrencyLevel = 8;

    private static readonly Collection<TimeSpan> DefaultRetries = new() { TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(4) };

    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<DataflowWebHookSender> _logger;
    private readonly ActionBlock<WebHookWorkItem>[] _launchers;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public DataflowWebHookSender(IHttpClientFactory clientFactory, ILogger<DataflowWebHookSender> logger) : base(logger)
    {
        _clientFactory = clientFactory;
        _logger = logger;
        _cancellationTokenSource = new CancellationTokenSource();

        var options = new ExecutionDataflowBlockOptions()
        {
            MaxDegreeOfParallelism = DefaultMaxConcurrencyLevel,
            CancellationToken = _cancellationTokenSource.Token
        };

        var retryDelays = DefaultRetries;

        _launchers = new ActionBlock<WebHookWorkItem>[1 + retryDelays.Count];
        var offset = 0;
        _launchers[offset++] = new ActionBlock<WebHookWorkItem>(async item => await LaunchWebHook(item, _cancellationTokenSource.Token), options);
        foreach (var delay in retryDelays)
        {
            _launchers[offset++] = new ActionBlock<WebHookWorkItem>(async item => await DelayedLaunchWebHook(item, delay, _cancellationTokenSource.Token), options);
        }

        _logger.LogInformation($"Start WebHookSender with a total of {_launchers.Length} attempt(s) of sending WebHooks");
    }

    public override Task SendWebHookWorkItemsAsync(IEnumerable<WebHookWorkItem> workItems)
    {
        if (workItems == null)
        {
            throw new ArgumentNullException(nameof(workItems));
        }

        var tasks = workItems.Select(workItem => _launchers[0].SendAsync(workItem)).ToArray();

        return Task.WhenAll(tasks);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                _cancellationTokenSource.Cancel();
                var completionTasks = _launchers.Select(launcher =>
                {
                    launcher.Complete();
                    return launcher.Completion;
                }).ToArray();

                Task.WaitAll(completionTasks);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                ex = ex.GetBaseException();
                var message = $"CompletionFailure: {ex.Message}";
                _logger.LogError(message, ex);
            }
            _cancellationTokenSource.Dispose();
        }
        base.Dispose(disposing);
    }

    private async Task DelayedLaunchWebHook(WebHookWorkItem item, TimeSpan delay, CancellationToken cancellationToken)
    {
        await Task.Delay(delay, cancellationToken);
        if (cancellationToken.IsCancellationRequested) return;
        await LaunchWebHook(item, cancellationToken);
    }

    private async Task LaunchWebHook(WebHookWorkItem workItem, CancellationToken cancellationToken)
    {
        try
        {
            var request = CreateWebHookRequest(workItem);
            var httpClient = _clientFactory.CreateClient();
            var response = await httpClient.SendAsync(request, cancellationToken);

            _logger.LogInformation($"WebHook '{workItem.WebHook.Id}' resulted in status code '{response.StatusCode}' on attempt '{workItem.Offset}'.");

            if (response.IsSuccessStatusCode)
            {
                return;
            }

            if (response.StatusCode == HttpStatusCode.Gone)
            {
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to submit attempt {workItem.Offset} of WebHook {workItem.WebHook.Id} due to failure: {ex.Message}.");
        }

        if (cancellationToken.IsCancellationRequested) return;

        try
        {
            workItem.Offset++;
            if (workItem.Offset < _launchers.Length)
            {
                await _launchers[workItem.Offset].SendAsync(workItem);
            }
            else
            {
                _logger.LogError($"Giving up sending WebHook '{workItem.WebHook.Id}' after '{workItem.Offset}' attempts.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to submit attempt {workItem.Offset} of WebHook {workItem.WebHook.Id} due to failure: {ex.Message}");
        }
    }
}