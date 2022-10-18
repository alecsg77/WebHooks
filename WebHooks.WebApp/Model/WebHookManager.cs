namespace WebHooks.WebApp.Model
{
    public class WebHookManager : IWebHookManager
    {
        private readonly IWebHookRepository _webHookStore;
        private readonly IWebHookSender _webHookSender;

        public WebHookManager(IWebHookRepository webHookStore, IWebHookSender webHookSender)
        {
            _webHookStore = webHookStore;
            _webHookSender = webHookSender;
        }

        public async Task<int> NotifyAsync(string user, string message)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var webHooks = await _webHookStore.QueryWebHooksAsync(user);

            var workItems = GetWorkItems(webHooks, message);

            await _webHookSender.SendWebHookWorkItemsAsync(workItems);
            return webHooks.Count;
        }

        internal static IEnumerable<WebHookWorkItem> GetWorkItems(ICollection<WebHook> webHooks, string notifications)
        {
            return webHooks.Select(webHook => new WebHookWorkItem(webHook, notifications)).ToList();
        }
    }
}