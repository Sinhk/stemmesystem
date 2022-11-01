using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.SessionStorage;
using IdentityModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Stemmesystem.Shared;

namespace Stemmesystem.Client;

public class CustomAuthenticationProvider : RemoteAuthenticationService<RemoteAuthenticationState, RemoteUserAccount, ApiAuthorizationProviderOptions>
{
    private readonly ISessionStorageService _sessionStorage;

    public CustomAuthenticationProvider(IJSRuntime jsRuntime, IOptionsSnapshot<RemoteAuthenticationOptions<ApiAuthorizationProviderOptions>> options, NavigationManager navigation, AccountClaimsPrincipalFactory<RemoteUserAccount> accountClaimsPrincipalFactory, ISessionStorageService sessionStorage) : base(jsRuntime, options, navigation, accountClaimsPrincipalFactory )
    {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await GetToken();
        if (token == null)
            return await base.GetAuthenticationStateAsync();

        if (token.ValidTo < DateTime.UtcNow)
            return new AuthenticationState(new ClaimsPrincipal());

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(token.Claims,AuthConstants.DelegatkodeGrantType,JwtClaimTypes.Name,JwtClaimTypes.Role));
        return new AuthenticationState(claimsPrincipal);
    }

    private async Task<JwtSecurityToken?> GetToken()
    {
        var token = await _sessionStorage.GetItemAsStringAsync("token");
        if (string.IsNullOrEmpty(token))
            return null;
        var securityToken = new JwtSecurityToken(token);
        if (securityToken.ValidTo < DateTime.UtcNow)
            return null;
        return securityToken;
    }


    public override async ValueTask<AccessTokenResult> RequestAccessToken()
    {
        var token = await GetToken();
        if (token == null)
            return await base.RequestAccessToken();
        return new AccessTokenResult(AccessTokenResultStatus.Success, new AccessToken {Expires = token.ValidTo, Value = token.RawData, GrantedScopes = token.Audiences.ToList()}, "");
    }

    public override async ValueTask<AccessTokenResult> RequestAccessToken(AccessTokenRequestOptions options)
    {
        var token = await GetToken();
        if (token == null)
            return await base.RequestAccessToken(options);
        return new AccessTokenResult(AccessTokenResultStatus.Success, new AccessToken {Expires = token.ValidTo, Value = token.RawData, GrantedScopes = token.Audiences.ToList()}, "");
    }


    public void Notify()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}