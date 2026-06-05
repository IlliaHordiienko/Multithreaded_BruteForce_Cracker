using System;
using System.Security.Cryptography;
using System.Text;

namespace BruteForceCracker
{
    public class PasswordManager
    {
        // Constant static salt used during hashing
        private const string STATIC_SALT = "StaticSaltValue123!";

        // Character set utilized for password generation
        public const string ALPHABET = "abcdefghijklmnopqrstuvwxyz";

        // Generates random lowercase password with length [4-6)
        public static string GenerateRandomPassword()
        {
            Random rand = new Random();
            int length = rand.Next(4, 6);

            StringBuilder password = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                int index = rand.Next(ALPHABET.Length);
                password.Append(ALPHABET[index]);
            }
            return password.ToString();
        }

        // Computes SHA-256 hash of password combined with static salt
        public static string ComputeHash(string plainTextPassword)
        {
            string saltedPassword = plainTextPassword + STATIC_SALT;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
