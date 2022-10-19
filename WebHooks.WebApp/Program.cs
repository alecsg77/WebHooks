using Duende.IdentityServer;

using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

using WebHooks.WebApp.Data;
using WebHooks.WebApp.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentityServer()
    .AddApiAuthorization<IdentityUser, ApplicationDbContext>(options =>
    {
        options.Clients.AddIdentityServerSPA("swagger", c =>
        {
            c.WithRedirectUri("/swagger/oauth2-redirect.html");
        });
    });

builder.Services.AddAuthentication()
    .AddIdentityServerJwt();

builder.Services.Configure<JwtBearerOptions>(IdentityServerJwtConstants.IdentityServerJwtBearerScheme, options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            string authorization = context.Request.Headers[HeaderNames.Authorization];
            return string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer ")
                ? IdentityConstants.ApplicationScheme
                : null;
        };
    });

builder.Services.AddMvc();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("openId", new OpenApiSecurityScheme()
    {
        Type = SecuritySchemeType.OpenIdConnect,
        OpenIdConnectUrl = new Uri("/.well-known/openid-configuration", UriKind.Relative)
    });
    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme{
                    Reference = new OpenApiReference{
                        Id = "openId",
                        Type = ReferenceType.SecurityScheme
                    }
                },
                new [] {
                    "WebHooks.WebAppAPI"
                }
            }
        });
});

builder.Services.AddHttpClient();

builder.Services.AddScoped<IWebHookRegistrationsManager, WebHookRegistrationsManager>();
builder.Services.AddScoped<IWebHookUser, WebHookUser>();
builder.Services.AddScoped<IWebHookRepository, DbWebHookRepository>();
builder.Services.AddScoped<IWebHookManager, WebHookManager>();
builder.Services.AddSingleton<IWebHookSender, DataflowWebHookSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId("swagger");
        options.OAuthScopes(
            IdentityServerConstants.StandardScopes.OpenId,
            IdentityServerConstants.StandardScopes.Profile,
            "WebHooks.WebAppAPI"
        );
        options.OAuthUsePkce();
    });
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseIdentityServer();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.CreateDatabase<ApplicationDbContext>();

app.Run();
