using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace WebHooks.WebApp.Data
{
    [Table("WebHooks")]
    public class Registration
    {
        [StringLength(256)]
        public string? User { get; set; }

        [StringLength(64)]
        public string? Id { get; set; }

        [Required]
        public string? Data { get; set; }

        public int RowVer { get; set; }
    }
}
