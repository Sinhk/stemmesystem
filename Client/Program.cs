using Blazored.SessionStorage;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Stemmesystem.Client;
using Stemmesystem.Client.Services.CSV;
using Stemmesystem.Client.SignalR;
using Stemmesystem.Core.Interfaces;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTransient<CookieHandler>();
builder.Services.AddHttpClient(HttpClientNames.Auth, opt => opt.BaseAddress = new Uri(builder.Configuration["AuthUrl"]!))
    .AddHttpMessageHandler<CookieHandler>();

builder.Services.AddHttpClient(HttpClientNames.Api, client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddHttpClient(nameof(DelegatkodeAuthService), client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Stemmesystem.ServerAPI"));

builder.Services.AddAutoMapper(typeof(WebAutoMapperProfile));
builder.Services.AddSingleton<CsvImport>();

#region Grpc

builder.Services.AddScoped(services => 
{ 
    var baseAddressMessageHandler = services.GetRequiredService<BaseAddressAuthorizationMessageHandler>();
    baseAddressMessageHandler.InnerHandler = new HttpClientHandler();
    var grpcWebHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, baseAddressMessageHandler);
    var channel = GrpcChannel.ForAddress(builder.HostEnvironment.BaseAddress, new GrpcChannelOptions { HttpHandler = grpcWebHandler });
    return channel; 
});

builder.Services.AddGrpcClient<IArrangementService>();
builder.Services.AddGrpcClient<IDelegatService>();
builder.Services.AddGrpcClient<IAdminDelegatService>();
builder.Services.AddGrpcClient<IStemmeService>();
builder.Services.AddGrpcClient<IAdminStemmeService>();
builder.Services.AddGrpcClient<ISakService>();
builder.Services.AddGrpcClient<IPinSender>();

#endregion

builder.Services.AddBlazoredSessionStorage();
builder.Services.AddScoped<IDelegatkodeAuthService, DelegatkodeAuthService>();
builder.Services.AddScoped<IDelegatNotifierService, DelegatNotifierService>();
builder.Services.AddScoped<IAdminNotifierService, AdminNotifierService>();

builder.Services.AddApiAuthorization();

await builder.Build().RunAsync();