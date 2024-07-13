using System.Security.Cryptography;

namespace hoistmt.Services;

public static class TokenGenerator
{
    public static string GenerateToken()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] tokenData = new byte[32];
            rng.GetBytes(tokenData);

            return Convert.ToBase64String(tokenData);
        }
    }
}