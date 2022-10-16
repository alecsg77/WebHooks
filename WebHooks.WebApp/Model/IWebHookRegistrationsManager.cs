using System.Security.Claims;
using System.Security.Principal;

namespace WebHooks.WebApp.Model
{
    public interface IWebHookRegistrationsManager
    {
        Task<IEnumerable<WebHook>> GetWebHooksAsync(IPrincipal user);
        Task<WebHook?> LookupWebHookAsync(IPrincipal user, string id);
        Task<RegistrationResult> RegisterWebHookAsync(IPrincipal user, WebHook webHook);
        Task<RegistrationResult> UpdateWebHookAsync(ClaimsPrincipal user, WebHook webHook);
        Task<RegistrationResult> CancelWebHookAsync(IPrincipal user, string id);
        Task CancelAllWebHooksAsync(IPrincipal user);
    }
}