using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace WebHooks.WebApp.Data
{
    public class ApplicationDbContext : ApiAuthorizationDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IOptions<OperationalStoreOptions> operationalStoreOptions)
            : base(options, operationalStoreOptions)
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