using InDuckTor.Shared.Security.Jwt;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace InDuckTor.Vestnik.Api.Configuration.Auth;

internal static class TokenValidationFactory
{
    internal static TokenValidationParameters CreateTokenValidationParameters(JwtSettings settings)
    {
        return new()
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = false,
            RequireSignedTokens = false,
            RequireExpirationTime = true,
            ValidIssuer = settings.Issuer,
            ValidAudience = settings.Audience,
            IssuerSigningKey = null,
            SignatureValidator = settings.OmitSignature
                ? (token, _) => new JsonWebToken(token)
                : null
        };
    }
}