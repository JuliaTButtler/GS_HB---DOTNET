using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace HidroBairro.API.Auth;

public static class JwtKeyHelper
{
    public static SymmetricSecurityKey CreateSigningKey(AuthOptions auth)
    {
        if (string.IsNullOrWhiteSpace(auth.Key))
            throw new InvalidOperationException("Auth:Key não configurada.");

        byte[] keyBytes;
        if (auth.KeyIsBase64)
        {
            try
            {
                keyBytes = Convert.FromBase64String(auth.Key.Trim());
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException(
                    "Auth:KeyIsBase64=true, mas Auth:Key não é Base64 válido.", ex);
            }
        }
        else
        {
            keyBytes = Encoding.UTF8.GetBytes(auth.Key);
        }

        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException(
                "Auth:Key deve resultar em pelo menos 32 bytes (HS256) após decode/UTF-8.");
        }

        return new SymmetricSecurityKey(keyBytes);
    }
}
