using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;

using TCPGameServer.Control.Output;

namespace TCPGameServer.World.Players.PlayerFiles
{
    class PasswordHashing
    {
        public static byte[] generateSalt()
        {
            // Define min and max salt sizes.
            int minSaltSize = 4;
            int maxSaltSize = 8;

            // Generate a random number for the size of the salt.
            Random random = new Random();
            int saltSize = random.Next(minSaltSize, maxSaltSize);

            // Allocate a byte array, which will hold the salt.
            byte[] saltBytes = new byte[saltSize];

            // Initialize a random number generator.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

            // Fill the salt with cryptographically strong byte values.
            rng.GetNonZeroBytes(saltBytes);

            return saltBytes;
        }

        public static string Rehash(string passwordHashedByClient)
        {
            // Convert plain text into a byte array.
            byte[] passwordBytes = Convert.FromBase64String(passwordHashedByClient);

            HashAlgorithm hash = new SHA512Managed();

            // Compute hash value of our plain text with appended salt.
            byte[] hashBytes = hash.ComputeHash(passwordBytes);

            // Convert result into a base64-encoded string.
            string passwordRehashed = Convert.ToBase64String(hashBytes);

            return passwordRehashed;
        }

        public static bool VerifyPassword(string passwordHashedByClient, string hashValueInSave)
        {
            string passwordRehashed = Rehash(passwordHashedByClient);

            return (passwordRehashed.Equals(hashValueInSave));
        }
    }
}