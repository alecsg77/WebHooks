using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebHooks.WebApp.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebHooks.WebApp.Controllers
{
    [Route("api/webhooks/registrations")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class WebHookRegistrationsController : ControllerBase
    {
        private readonly IWebHookRegistrationsManager _registrationsManager;

        public WebHookRegistrationsController(IWebHookRegistrationsManager registrationsManager)
        {
            _registrationsManager = registrationsManager;
        }

        [HttpGet]
        public async Task<IEnumerable<WebHook>> Get()
        {
            var webHooks = await _registrationsManager.GetWebHooksAsync(User);
            return webHooks;
        }

        [HttpGet("{id}", Name = "RegistrationLookupAction")]
        public async Task<ActionResult<WebHook>> Lookup(string id)
        {
            var webHook = await _registrationsManager.LookupWebHookAsync(User, id);
            if (webHook == null)
            {
                return NotFound();
            }

            return webHook;
        }

        [HttpPost]
        public async Task<ActionResult> Register([FromBody] WebHook? webHook)
        {
            if (webHook == null)
            {
                return BadRequest();
            }

            var result = await _registrationsManager.RegisterWebHookAsync(User, webHook);
            if (result == RegistrationResult.Success)
            {
                return CreatedAtRoute("RegistrationLookupAction", new { id = webHook.Id }, webHook);
            }

            return Result(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(string id, [FromBody] WebHook? webHook)
        {
            if (webHook == null)
            {
                return BadRequest();
            }

            if (!string.Equals(id, webHook.Id, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest();
            }

            var result = await _registrationsManager.UpdateWebHookAsync(User, webHook);
            return Result(result);

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Cancel(string id)
        {
            var result = await _registrationsManager.CancelWebHookAsync(User, id);
            return Result(result);
        }

        [HttpDelete()]
        public async Task CancelAll()
        {
            await _registrationsManager.CancelAllWebHooksAsync(User);
        }

        private ActionResult Result(RegistrationResult result)
        {
            return result switch
            {
                RegistrationResult.Success => Ok(),
                RegistrationResult.Conflict => Conflict(),
                RegistrationResult.NotFound => NotFound(),
                RegistrationResult.OperationError => BadRequest(),
                _ => Problem()
            };
        }
    }
}
