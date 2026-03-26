using System.IdentityModel.Tokens.Jwt;
using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.Storage;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services.KeyManagement;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ProtoBuf.Grpc.Server;
using Stemmesystem.Api;
using Stemmesystem.Data;
using Stemmesystem.Data.Models;
using Stemmesystem.Data.Repositories;
using Stemmesystem.Server;
using Stemmesystem.Server.Auth;
using Stemmesystem.Server.Data.Repositories;
using Stemmesystem.Server.Features.MinSpeiding;
using Stemmesystem.Server.Hubs;
using Stemmesystem.Server.InternalServices;
using Stemmesystem.Server.Services;
using Stemmesystem.Shared;
using Stemmesystem.Shared.Tools;
using ZiggyCreatures.Caching.Fusion;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;
using Secret = Duende.IdentityServer.Models.Secret;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAppDbContext();
builder.Services.AddOperationalDbContext<StemmesystemContext>(options =>
{
    options.EnableTokenCleanup = true;
    options.RemoveConsumedTokens = true;
    options.TokenCleanupInterval = 3600; // interval in seconds (default is 3600)
});
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<StemmesystemContext>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        // MaxLengthForKeys = 0 keeps composite key columns as `text` (PostgreSQL unlimited)
        // matching the design-time migration snapshot. AddDefaultIdentity sets 128 by default
        // which would cause a PendingModelChangesWarning at runtime.
        options.Stores.MaxLengthForKeys = 0;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<StemmesystemContext>();

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services.AddIdentityServer()
    .AddExtensionGrantValidator<KodeExtensionGrantValidator>()
    .AddAspNetIdentity<ApplicationUser>()
    .AddInMemoryIdentityResources(new IdentityResource[]
    {
        new IdentityResources.OpenId { UserClaims = { "name", "role" } },
        new IdentityResources.Profile(),
    })
    .AddInMemoryApiResources(new ApiResource[]
    {
        new ApiResource("Stemmesystem.ServerAPI")
        {
            UserClaims = { "name", "role" },
            Scopes = { "Stemmesystem.ServerAPI" },
        },
    })
    .AddInMemoryApiScopes(new ApiScope[]
    {
        new ApiScope("Stemmesystem.ServerAPI"),
    })
    .AddInMemoryClients(new Client[]
    {
        new Client
        {
            ClientId = "Stemmesystem.Client",
            AllowedGrantTypes = GrantTypes.Code,
            RequireClientSecret = false,
            RequirePkce = true,
            AllowAccessTokensViaBrowser = true,
            AllowedScopes = { "openid", "profile", "Stemmesystem.ServerAPI" },
        },
        new Client
        {
            ClientId = AuthConstants.DelegatkodeClientId,
            ClientSecrets = { new Secret("passord".Sha256()) },
            AllowedGrantTypes = { AuthConstants.DelegatkodeGrantType },
            AllowedScopes = { "openid", "profile", "Stemmesystem.ServerAPI" },
        },
    })
    .AddProfileService<StemmeProfileService>()
    .AddInMemoryCaching();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRedirectUriValidator, SameOriginRedirectUriValidator>();

builder.Services.RemoveAll(typeof(IValidationKeysStore));
builder.Services.RemoveAll(typeof(ISigningCredentialStore));
builder.Services.AddTransient<ISigningCredentialStore>(serviceProvider => serviceProvider.GetRequiredService<IAutomaticKeyManagerKeyStore>());
builder.Services.AddTransient<IValidationKeysStore>(serviceProvider => serviceProvider.GetRequiredService<IAutomaticKeyManagerKeyStore>());

// Use a policy scheme that forwards Bearer token requests to the LocalApi handler
// and all other requests to the cookie scheme — replacing the scheme selector that
// Microsoft.AspNetCore.ApiAuthorization.IdentityServer registered via AddIdentityServerJwt().
const string SmartScheme = "SmartScheme";
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = SmartScheme;
})
.AddPolicyScheme(SmartScheme, SmartScheme, options =>
{
    options.ForwardDefaultSelector = ctx =>
    {
        string? authorization = ctx.Request.Headers.Authorization;
        if (!string.IsNullOrEmpty(authorization) &&
            authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return IdentityServerConstants.LocalApi.AuthenticationScheme;
        return IdentityConstants.ApplicationScheme;
    };
})
.AddGoogle("Google", options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
})
.AddLocalApi();

builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHealthChecks();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

builder.Services.AddCodeFirstGrpc();


builder.Services.AddScoped<IDelegatRepository, DelegatRepository>();
builder.Services.AddScoped<IArrangementRepository, ArrangementRepository>();
builder.Services.AddScoped<DelegatService>();
builder.Services.AddScoped<NotificationManager>();
builder.Services.AddScoped<MinSpeidingService>();
builder.Services.AddScoped<UserManager>();

builder.Services.AddHttpClient<ISmsSender, SveveSmsSender>();
builder.Services.AddOptions<SveveOptions>()
    .BindConfiguration("Sveve")
    .ValidateDataAnnotations();
    
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<IEpostSender, EmailSender>();
builder.Services.AddOptions<EmailSettings>()
    .BindConfiguration("EmailSettings")
    .ValidateDataAnnotations();

builder.Services.AddSingleton<IKeyGenerator, RngKeyGenerator>();
builder.Services.AddSingleton<IKeyHasher, KeyHasher>();

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<ApiAutoMapperProfile>());
builder.Services.AddFusionCache()
    .WithDefaultEntryOptions(new FusionCacheEntryOptions {
        Duration = TimeSpan.FromMinutes(1),
    
        IsFailSafeEnabled = true,
        FailSafeMaxDuration = TimeSpan.FromHours(2),
        FailSafeThrottleDuration = TimeSpan.FromSeconds(30),

        // FACTORY TIMEOUTS
        FactorySoftTimeout = TimeSpan.FromMilliseconds(100),
        FactoryHardTimeout = TimeSpan.FromSeconds(10)
    });

builder.Services.AddAuthorizationCore(b => b.AddPolicy("admin", policyBuilder => policyBuilder.RequireRole("admin")));

var app = builder.Build();
await MigrateDatabase(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    var mapperConfig = app.Services.GetRequiredService<IConfigurationProvider>();
    mapperConfig.AssertConfigurationIsValid();
    
    app.UseMigrationsEndPoint();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseResponseCompression();
    app.UseExceptionHandler("/Error");
    app.UseHttpsRedirection();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

// Extract the SignalR access token from the query string for hub requests and move it
// to the Authorization header so that AddLocalApi() can authenticate the connection.
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/hubs"))
    {
        var accessToken = context.Request.Query["access_token"].ToString();
        if (!string.IsNullOrEmpty(accessToken))
            context.Request.Headers.Authorization = $"Bearer {accessToken}";
    }
    await next();
});

app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

app.MapRazorPages();
app.MapGrpcServices();
app.MapControllers();
app.MapHealthChecks("/healthz");

MinSpeidingEndpoints.MapMinSpeidingEndpoints(app);
    
app.MapHub<DelegatHub>("/hubs/delegat");
app.MapHub<AdminHub>("/hubs/admin");
app.MapFallbackToFile("index.html");

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    var url = $"http://0.0.0.0:{port}";
    app.Run(url);
}
else
{
    app.Run();
}

return;

async Task MigrateDatabase(WebApplication webApplication)
{
    await using var scope = webApplication.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<StemmesystemContext>();

    await db.Database.MigrateAsync();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager>();
    await userManager.AddMissingAdminUsers();
}

public partial class Program { }