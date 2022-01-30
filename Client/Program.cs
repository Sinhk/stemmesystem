using System.IdentityModel.Tokens.Jwt;
using Blazored.SessionStorage;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Stemmesystem.Client;
using Stemmesystem.Shared;
using Stemmesystem.Shared.Interfaces;
using Stemmesystem.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("Stemmesystem.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
builder.Services.AddHttpClient(nameof(DelegatkodeAuthService), client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Stemmesystem.ServerAPI"));

builder.Services.AddAutoMapper(typeof(WebAutoMapperProfile));

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

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddScoped<IDelegatkodeAuthService, DelegatkodeAuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationProvider>();


builder.Services.AddApiAuthorization();

await builder.Build().RunAsync();
