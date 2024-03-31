using System.Net.Http.Headers;
using InDuckTor.Shared.Security.Context;
using InDuckTor.Shared.Security.Jwt;
using LazyCache;

namespace InDuckTor.Vestnik.Api.Services;

public class InDuckTorSystemTokenHandler : DelegatingHandler
{
    public const string SystemAccessTokenCacheKey = "access-token/in-duck-tor/system";

    private readonly ITokenFactory _tokenFactory;
    private readonly IAppCache _cache;

    public InDuckTorSystemTokenHandler(ITokenFactory tokenFactory, IAppCache cache)
    {
        _tokenFactory = tokenFactory;
        _cache = cache;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = await _cache.GetOrAddAsync(SystemAccessTokenCacheKey, async cacheEntry =>
        {
            TimeSpan expiration = TimeSpan.FromMinutes(5);
            cacheEntry.AbsoluteExpirationRelativeToNow = expiration;
            var systemUser = UserContext.Create(
                id: 0,
                login: "in-duck-tor",
                clientId: "in-duck-tor",
                accountType: AccountType.System,
                permissions: Array.Empty<string>());

            return await _tokenFactory.CreateToken(systemUser.Claims, expiration, cancellationToken);
        });

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await base.SendAsync(request, cancellationToken);
    }
}