using System.Text;
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
            ValidateIssuerSigningKey = !settings.OmitSignature,
            RequireSignedTokens = !settings.OmitSignature,
            RequireExpirationTime = true,
            ValidIssuer = settings.Issuer,
            ValidAudience = settings.Audience,
            IssuerSigningKey = settings.OmitSignature
                ? null
                : settings.SignatureType == EncryptionType.Asymmetric
                    ? RsaKeyGenerator.GenerateKey(settings.SecretKeyPath)
                    : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey)),
            SignatureValidator = settings.OmitSignature
                ? (token, _) => new JsonWebToken(token)
                : null
        };
    }
}