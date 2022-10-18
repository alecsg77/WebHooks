namespace WebHooks.WebApp.Model
{
    public interface IWebHookManager
    {
        Task<int> NotifyAsync(string user, string message);
    }
}