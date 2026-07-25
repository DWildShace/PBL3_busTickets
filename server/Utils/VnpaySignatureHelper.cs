using System.Security.Cryptography;
using System.Text;

namespace Pbl3.Utils
{
    public static class VnpaySignatureHelper
    {
        public static string ComputeHmacSha512(string secretKey, string rawData)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var rawBytes = Encoding.UTF8.GetBytes(rawData);
            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(rawBytes);
            return Convert.ToHexString(hashBytes).ToUpperInvariant();
        }
    }
}
