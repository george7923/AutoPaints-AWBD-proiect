using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LicentaInAngular.Server.Utils
{
    public static class EncryptionHelper
    {
        private static readonly string encryptionKey = "SuperSecretKey123!"; 

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            using (Aes aes = Aes.Create())
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey);
                byte[] iv = new byte[aes.BlockSize / 8];

                using (var encryptor = aes.CreateEncryptor(keyBytes, iv))
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

                    return Convert.ToBase64String(iv) + ":" + Convert.ToBase64String(encryptedBytes);
                }
            }
        }


        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            if (!encryptedText.Contains(":"))
            {

                return encryptedText;
            }

            string[] parts = encryptedText.Split(':');
            if (parts.Length != 2)
                throw new FormatException("Invalid encrypted text format");

            using (Aes aes = Aes.Create())
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey);
                byte[] iv = Convert.FromBase64String(parts[0]);
                byte[] encryptedBytes = Convert.FromBase64String(parts[1]);

                using (var decryptor = aes.CreateDecryptor(keyBytes, iv))
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
            
        }
        #region Card Encryption Using PBKDF2 + AES-256
        public static string EncryptCARD(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            // 1. Generam un salt random de 16 bytes
            byte[] salt = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);

            // 2. Derivam cheia de 32 bytes pentru AES-256
            using var derive = new Rfc2898DeriveBytes(encryptionKey, salt, 10000, HashAlgorithmName.SHA256);
            byte[] key = derive.GetBytes(32);

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = key;
            aes.GenerateIV();

            // 3. Construim criptarea: [salt][iv][ciphertext]
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();

            // Prefixam salt-ul si IV-ul in clar
            ms.Write(salt, 0, salt.Length);
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs, Encoding.UTF8))
            {
                sw.Write(plainText);
            }

            // 4. Rezultatul este Base64(data)
            return Convert.ToBase64String(ms.ToArray());
        }

        public static string DecryptCARD(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            // 1. Obtinem buffer-ul complet
            byte[] fullBuffer = Convert.FromBase64String(cipherText);

            // 2. Extragem salt-ul (0..15), IV-ul (16..31) si ciphertext-ul rest
            byte[] salt = fullBuffer.Take(16).ToArray();
            byte[] iv = fullBuffer.Skip(16).Take(16).ToArray();
            byte[] cipher = fullBuffer.Skip(32).ToArray();

            // 3. Refacem cheia cu același salt si parametri PBKDF2
            using var derive = new Rfc2898DeriveBytes(encryptionKey, salt, 10000, HashAlgorithmName.SHA256);
            byte[] key = derive.GetBytes(32);

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipher);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, Encoding.UTF8);

            return sr.ReadToEnd();
        }
        #endregion
    }
}
