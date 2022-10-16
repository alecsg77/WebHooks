using System.ComponentModel.DataAnnotations;

namespace WebHooks.WebApp.Model
{
    public class WebHook
    {
        public WebHook()
        {
            Id = GetId();
        }

        public string Id { get; set; }

        [Required]
        public Uri? WebHookUri { get; set; }

        public string? Description { get; set; }

        public static string GetId()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
