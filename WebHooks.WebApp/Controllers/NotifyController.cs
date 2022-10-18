using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebHooks.WebApp.Model;

namespace WebHooks.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotifyController : ControllerBase
    {
        private readonly IWebHookUser _webHookUser;
        private readonly IWebHookManager _webHookManager;

        public NotifyController(IWebHookUser webHookUser, IWebHookManager webHookManager)
        {
            _webHookUser = webHookUser;
            _webHookManager = webHookManager;
        }

        [HttpPost]
        public async Task<int> Notify([FromBody]string message)
        {
            var userId = await _webHookUser.GetUserIdAsync(User);
            return await _webHookManager.NotifyAsync(userId, message);
        }
    }
}
