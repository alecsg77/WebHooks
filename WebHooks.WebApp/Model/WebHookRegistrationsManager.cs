using System.Security.Claims;
using System.Security.Principal;

namespace WebHooks.WebApp.Model
{
    public class WebHookRegistrationsManager : IWebHookRegistrationsManager
    {
        private readonly IWebHookRepository _repository;
        private readonly IWebHookUser _userManager;

        public WebHookRegistrationsManager(IWebHookRepository repository, IWebHookUser userManager)
        {
            _repository = repository;
            _userManager = userManager;
        }

        public async Task<IEnumerable<WebHook>> GetWebHooksAsync(IPrincipal user)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            IEnumerable<WebHook> webHooks = await _repository.GetAllWebHooksAsync(userId);
            return webHooks;
        }

        public async Task<WebHook?> LookupWebHookAsync(IPrincipal user, string id)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var webHook = await _repository.LookupWebHookAsync(userId, id);
            return webHook;
        }

        public async Task<RegistrationResult> RegisterWebHookAsync(IPrincipal user, WebHook webHook)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            var userId = await _userManager.GetUserIdAsync(user);

            VerifyId(webHook);

            var result = await _repository.InsertWebHookAsync(userId, webHook);
            return result;
        }

        public async Task<RegistrationResult> UpdateWebHookAsync(ClaimsPrincipal user, WebHook webHook)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            var userId = await _userManager.GetUserIdAsync(user);

            var result = await _repository.UpdateWebHookAsync(userId, webHook);
            return result;
        }

        public async Task<RegistrationResult> CancelWebHookAsync(IPrincipal user, string id)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var result = await _repository.DeleteWebHookAsync(userId, id);
            return result;
        }

        public async Task CancelAllWebHooksAsync(IPrincipal user)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            await _repository.DeleteAllWebHooksAsync(userId);
        }
        private static void VerifyId(WebHook webHook)
        {
            if (string.IsNullOrEmpty(webHook.Id))
            {
                webHook.Id = WebHook.GetId();
            }
        }

    }
}