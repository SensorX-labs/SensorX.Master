using System.Security.Cryptography;
using System.Text;

namespace SensorX.Master.Domain.Common.Utilities
{
    public static class HashHelper
    {
        public static string HashToken(string token)
        {
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        public static string GenerateSecureToken(int length)
        {
            var randomNumber = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}

