using Blazored.SessionStorage;
using Duende.IdentityModel.Client;
using Microsoft.AspNetCore.Components.Authorization;
using Stemmesystem.Shared;

namespace Stemmesystem.Client;

public interface IDelegatkodeAuthService
{
    Task<bool> Login(string delegatkode);
}

class DelegatkodeAuthService : IDelegatkodeAuthService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ISessionStorageService _sessionStorage;
    private readonly HttpClient _http;

    public DelegatkodeAuthService(IHttpClientFactory httpClientFactory, AuthenticationStateProvider authenticationStateProvider, ISessionStorageService sessionStorage)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _sessionStorage = sessionStorage;
        _http = httpClientFactory.CreateClient(nameof(DelegatkodeAuthService));
    }


    public async Task<bool> Login(string delegatkode)
    {
        var request = new TokenRequest
        {
            Address = "connect/token",
            GrantType = AuthConstants.DelegatkodeGrantType,
            ClientId = AuthConstants.DelegatkodeClientId,
            ClientSecret = "passord",
            Parameters =
            {
                {"delegatkode", delegatkode},
                {"scope", "openid profile Stemmesystem.ServerAPI" }
            }
        };
        var response = await _http.RequestTokenAsync(request);

        if (response.IsError) return false;
        
        await _sessionStorage.SetItemAsStringAsync("token", response.AccessToken);
        ((CustomAuthenticationProvider)_authenticationStateProvider).Notify();
        return true;

    }
}