using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services.KeyManagement;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProtoBuf.Grpc.Server;
using Stemmesystem.Api;
using Stemmesystem.Server;
using Stemmesystem.Server.Data;
using Stemmesystem.Server.Data.Repositories;
using Stemmesystem.Server.Models;
using Stemmesystem.Server.Services;
using Stemmesystem.Shared;
using Stemmesystem.Shared.Tools;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var provider = builder.Configuration.GetValue("Provider", "Sqlite");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()    
    .AddEntityFrameworkStores<ApplicationDbContext>()
   ;


JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services.AddIdentityServer()
    .AddExtensionGrantValidator<KodeExtensionGrantValidator>()
    .AddApiAuthorization<ApplicationUser, ApplicationDbContext>(options =>
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
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = ConfigureDb;
        options.EnableTokenCleanup = true;
        options.RemoveConsumedTokens = true;
        options.TokenCleanupInterval = 3600; // interval in seconds (default is 3600)
    })
    .AddInMemoryCaching();

builder.Services.RemoveAll(typeof(IValidationKeysStore));
builder.Services.RemoveAll(typeof(ISigningCredentialStore));
builder.Services.AddTransient<ISigningCredentialStore>(serviceProvider => serviceProvider.GetRequiredService<IAutomaticKeyManagerKeyStore>());
builder.Services.AddTransient<IValidationKeysStore>(serviceProvider => serviceProvider.GetRequiredService<IAutomaticKeyManagerKeyStore>());

builder.Services.AddAuthentication()
    .AddIdentityServerJwt();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddCodeFirstGrpc();

builder.Services.AddDbContext<StemmesystemContext>(ConfigureDb);

builder.Services.AddScoped<IDelegatRepository, DelegatRepository>();
builder.Services.AddScoped<IArrangementRepository, ArrangementRepository>();
builder.Services.AddScoped<DelegatService>();
builder.Services.AddScoped<NotificationManager>();

builder.Services.AddSingleton<IKeyGenerator, RngKeyGenerator>();
builder.Services.AddSingleton<IKeyHasher, KeyHasher>();

builder.Services.AddAutoMapper(typeof(ApiAutoMapperProfile));
builder.Services.AddLazyCache();

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Any, int.Parse(port)));

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StemmesystemContext>();
    var grantDb = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();

    await db.Database.MigrateAsync();
    await grantDb.Database.MigrateAsync();
    
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
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

app.MapRazorPages();
app.MapGrpcService<DelegatService>();
app.MapGrpcService<SakService>();
app.MapGrpcService<ArrangementService>();
app.MapGrpcService<StemmeService>();
app.MapGrpcService<PinSender>();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

void ConfigureDb(DbContextOptionsBuilder dbContextOptionsBuilder) =>
    _ = provider switch
    {
        "Sqlite" => dbContextOptionsBuilder.UseSqlite(connectionString, x => x.MigrationsAssembly("SqliteMigrations")),
        "SqlServer" => dbContextOptionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"), x => x.MigrationsAssembly("SqlServerMigrations")),
        "Postgres" => dbContextOptionsBuilder.UseNpgsql(ConnectionStringUtils.ParseHerokuPostgresString(), x=> x.MigrationsAssembly("PostgresMigrations")),
        _ => throw new Exception($"Unsupported provider: {provider}")
    };
