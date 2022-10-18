namespace WebHooks.WebApp.Model
{
    public class WebHookWorkItem
    {
        private string? _id;

        public WebHookWorkItem(WebHook webHook, string notifications)
        {
            WebHook = webHook;
            Notifications = notifications;
        }

        public string Id
        {
            get { return _id ??= Guid.NewGuid().ToString("N"); }
            set => _id = value;
        }
        public WebHook WebHook { get; }
        public string Notifications { get; }
        public int Offset { get; set; }
    }
}