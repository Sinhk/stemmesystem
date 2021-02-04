using System.IdentityModel.Tokens.Jwt;
using Duende.IdentityServer.EntityFramework.Storage;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services.KeyManagement;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ProtoBuf.Grpc.Server;
using Stemmesystem.Api;
using StemmeSystem.Data;
using StemmeSystem.Data.Models;
using StemmeSystem.Data.Repositories;
using Stemmesystem.Server;using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Server.Data.Repositories;
using Stemmesystem.Server.Hubs;
using Stemmesystem.Server.InternalServices;
using Stemmesystem.Server.Services;
using Stemmesystem.Shared;
using Stemmesystem.Shared.Tools;using Stemmesystem.Web;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;
using Secret = Duende.IdentityServer.Models.Secret;

var apiOptions = new MinSpeidingApiOptions
{
    ArrangementId = 3803,
    ApiKey = "f494be5c3cbc1a0ccb2ffd7d88a9852118980061"
};

using var httpClient = new HttpClient();
var speidingService = new MinSpeidingService(httpClient);
var members = await speidingService.GetArrangementParticipants(apiOptions);


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

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()    
    .AddEntityFrameworkStores<StemmesystemContext>()
   ;


JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services.AddIdentityServer()
    .AddExtensionGrantValidator<KodeExtensionGrantValidator>()
    .AddApiAuthorization<ApplicationUser, StemmesystemContext>(options =>
    {
        options.IdentityResources["openid"].UserClaims.Add("name");
        options.ApiResources.Single().UserClaims.Add("name");
        options.IdentityResources["openid"].UserClaims.Add("role");
        options.ApiResources.Single().UserClaims.Add("role");
        options.Clients.Add(new Client
        {
            ClientId = AuthConstants.DelegatkodeClientId, 
            ClientSecrets = new List<Secret>
            {
                new("passord".Sha256())
            },
            AllowedGrantTypes = {AuthConstants.DelegatkodeGrantType}
        });
    })
    .AddProfileService<StemmeProfileService>()
    .AddInMemoryCaching();

builder.Services.RemoveAll(typeof(IValidationKeysStore));
builder.Services.RemoveAll(typeof(ISigningCredentialStore));
builder.Services.AddTransient<ISigningCredentialStore>(serviceProvider => serviceProvider.GetRequiredService<IAutomaticKeyManagerKeyStore>());
builder.Services.AddTransient<IValidationKeysStore>(serviceProvider => serviceProvider.GetRequiredService<IAutomaticKeyManagerKeyStore>());

builder.Services.AddAuthentication()
    .AddGoogle("Google", options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    })
    .AddIdentityServerJwt();

builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>());

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

builder.Services.AddAutoMapper(typeof(ApiAutoMapperProfile));
builder.Services.AddLazyCache();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StemmesystemContext>();

    await db.Database.MigrateAsync();
    
    if (!db.Arrangement.Any())
    {
        var delegatService = scope.ServiceProvider.GetRequiredService<DelegatService>();
        await TestData.SeedData(db, delegatService);
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    if (!await userManager.Users.AnyAsync())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await TestData.CreateAdminUsers(userManager, roleManager);
    }
}

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

app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

app.MapRazorPages();
app.MapGrpcServices();
app.MapControllers();
app.MapHealthChecks("/healthz");
    
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

public partial class Program { }