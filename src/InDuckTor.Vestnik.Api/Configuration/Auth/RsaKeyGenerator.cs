using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace InDuckTor.Vestnik.Api.Configuration.Auth;

internal static class RsaKeyGenerator
{
    internal static RsaSecurityKey GenerateKey(string keyPath)
    {
        var rsa = RSA.Create();
        var xmlString = File.ReadAllText(keyPath);
        rsa.FromXmlString(xmlString);
        return new(rsa);
    }
}