namespace WebHooks.WebApp.Model
{
    public interface IWebHookSender
    {
        Task SendWebHookWorkItemsAsync(IEnumerable<WebHookWorkItem> workItems);
    }
}