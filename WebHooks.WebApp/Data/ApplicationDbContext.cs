using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebHooks.WebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Registration>()
                .HasKey(x => new { x.Id, x.User });
            builder.Entity<Registration>()
                .Property(x => x.RowVer)
                .HasDefaultValue(0)
                .IsRowVersion();
        }
    }
}