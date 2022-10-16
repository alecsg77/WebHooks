using System.Security.Principal;

namespace WebHooks.WebApp.Model
{
    public interface IWebHookUser
    {
        Task<string> GetUserIdAsync(IPrincipal user);
    }
}