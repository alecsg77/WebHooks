using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace WebHooks.WebApp.Model
{
    public class WebHook
    {
        private IDictionary<string, string>? _headers;

        public WebHook()
        {
            Id = GetId();
        }

        public string Id { get; set; }

        [Required]
        public Uri? WebHookUri { get; set; }

        public string? Description { get; set; }

        public IDictionary<string, string> Headers
        {
            get => _headers ??= new Dictionary<string, string>();
            set => _headers = value;
        }

        public static string GetId()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
