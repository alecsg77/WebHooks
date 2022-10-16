namespace WebHooks.WebApp.Model
{
    public interface IWebHookRepository: IDisposable
    {
        Task<ICollection<WebHook>> GetAllWebHooksAsync(string userId);
        Task<WebHook?> LookupWebHookAsync(string userId, string id);
        Task<RegistrationResult> InsertWebHookAsync(string userId, WebHook webHook);
        Task<RegistrationResult> UpdateWebHookAsync(string userId, WebHook webHook);
        Task<RegistrationResult> DeleteWebHookAsync(string userId, string id);
        Task DeleteAllWebHooksAsync(string userId);
    }
}