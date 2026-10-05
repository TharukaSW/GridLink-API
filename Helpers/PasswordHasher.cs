using System.Security.Cryptography;

namespace GridLink.Helpers
{
    public class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySIize = 32;
        private const int Iterations = 100000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;


        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                Algorithm,
                KeySIize
                );

            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            string[] parts = storedHash.Split('.');
            if (parts.Length != 2)
            {
                return false;
            }


            try
            {
                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] expectedHash = Convert.FromBase64String(parts[1]);

                byte[] actualPassword = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                Algorithm,
                KeySIize
                );

                return CryptographicOperations.FixedTimeEquals(actualPassword, expectedHash);
            }
            catch
            {
                return false;
            }


            //return false;
        }

    }
}
