using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ProtoBuf.Grpc.Server;
using Stemmesystem.Data;
using Stemmesystem.Data.Models;
using Stemmesystem.Data.Repositories;
using Stemmesystem.Server;
using Stemmesystem.Server.Data.Repositories;
using Stemmesystem.Server.Features.MinSpeiding;
using Stemmesystem.Server.Hubs;
using Stemmesystem.Server.InternalServices;
using Stemmesystem.Server.Services;
using Stemmesystem.Shared.Tools;
using ZiggyCreatures.Caching.Fusion;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAppDbContext();
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<StemmesystemContext>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()    
    .AddEntityFrameworkStores<StemmesystemContext>()
   ;

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.AddAuthentication();

builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>());

builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHealthChecks();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/octet-stream"]);
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

builder.Services.AddSingleton<IKeyHasher, KeyHasher>();

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

public partial class Program;