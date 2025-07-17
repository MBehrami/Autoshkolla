using System;
using System.Security.Cryptography;
using System.Text;
namespace AdminApi.Models.Helper
{
    public class PasswordHasher
    {
        public static string GenerateSalt(int size = 16)
        {
            var saltBytes = new byte[size];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
            /* using (var rng = new RandomNumberGenerator())
            {
                rng.GetBytes(saltBytes);
            } */
            return Convert.ToBase64String(saltBytes);
        }

        public static string HashPassword(string password, string salt, int iterations = 10000, int hashSize = 32)
        {
            var saltBytes = Convert.FromBase64String(salt);
            using (var rfc2898 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256))
            {
                var hashBytes = rfc2898.GetBytes(hashSize);
                return Convert.ToBase64String(hashBytes);
            }
        }

        public static bool VerifyPassword(string password, string salt, string hashedPassword, int iterations = 10000, int hashSize = 32)
        {
            var hashToVerify = HashPassword(password, salt, iterations, hashSize);
            return hashToVerify == hashedPassword;
        }
    }

}

