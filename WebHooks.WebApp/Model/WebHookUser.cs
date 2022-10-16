using System.Security.Claims;
using System.Security.Principal;

namespace WebHooks.WebApp.Model
{
    public class WebHookUser : IWebHookUser
    {
        public Task<string> GetUserIdAsync(IPrincipal user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            string? id = null;
            if (user is ClaimsPrincipal principal)
            {
                id = GetClaim(principal, ClaimTypes.Name) ?? GetClaim(principal, ClaimTypes.NameIdentifier);
            }

            if (id == null && user.Identity != null)
            {
                id = user.Identity.Name;
            }

            if (id == null)
            {
                throw new InvalidOperationException("No user");
            }

            return Task.FromResult(id);
        }

        private static string? GetClaim(ClaimsPrincipal? principal, string claimsType)
        {
            var claim = principal?.FindFirst(claimsType);
            return claim?.Value;
        }
    }
}