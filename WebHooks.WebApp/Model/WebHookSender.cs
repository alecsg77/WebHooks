using System.Text;

namespace WebHooks.WebApp.Model
{
    public abstract class WebHookSender : IWebHookSender, IDisposable
    {
        private readonly ILogger _logger;

        public WebHookSender(ILogger logger)
        {
            _logger = logger;
        }

        public abstract Task SendWebHookWorkItemsAsync(IEnumerable<WebHookWorkItem> workItems);

        protected virtual HttpRequestMessage CreateWebHookRequest(WebHookWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            var hook = workItem.WebHook;

            var request = new HttpRequestMessage(HttpMethod.Post, hook.WebHookUri)
            {
                Content = new StringContent(workItem.Notifications, Encoding.UTF8, "application/json")
            };

            foreach (var kvp in hook.Headers)
            {
                if (!request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value))
                {
                    if (!request.Content.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value))
                    {
                        var message = $"Invalid Header {kvp.Key} in WebHook {hook.Id}";
                        _logger.LogError(message);
                    }
                }
            }

            return request;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}