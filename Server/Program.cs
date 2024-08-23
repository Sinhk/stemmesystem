using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using ProtoBuf.Grpc.Server;
using Stemmesystem.Api;
using Stemmesystem.Data;
using Stemmesystem.Data.Models;
using Stemmesystem.Server;
using Stemmesystem.Server.Hubs;
using Stemmesystem.Server.Services;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAppDbContext();
builder.Services.AddAuthServices(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.AddSignalR();
builder.Services.AddHealthChecks();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/octet-stream"]);
});

builder.Services.AddCodeFirstGrpc();

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

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<StemmeUser>>();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<StemmeUser>();

app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

app.MapGrpcServices();
app.MapHealthChecks("/healthz").ShortCircuit();

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
public partial class Program;